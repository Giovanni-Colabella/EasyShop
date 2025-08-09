using API.Models.DTO;
using API.Models.Entities;
using API.Services;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace API.Models.Services.Infrastructure
{
    public class ExcelImportService
    {
        private readonly ApplicationDbContext _dbContext;

        public ExcelImportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ImportExcelResponseDto> ImportExcelAsync(
            Stream excelStream,
            int mappingHeaderId,
            char entityType,
            string fileName
        )
        {
            var mapping = await _dbContext.ExcelMappingHeaders
                .Include(h => h.ExcelMappingDetails)
                .FirstOrDefaultAsync(h => h.Id == mappingHeaderId && h.EntityType == entityType);

            if (mapping == null)
                throw new Exception("Non hai specificato nessun mapping valido per il documento.");

            var details = mapping.ExcelMappingDetails.ToList();
            var mappedColumns = new HashSet<string>(
                details.Select(d => d.ExcelColumnName),
                StringComparer.OrdinalIgnoreCase
            );

            var excelLog = new ExcelLog
            {
                ExcelMappingHeaderId = mappingHeaderId,
                EntityType = entityType,
                FileName = fileName,
                LogDetails = new List<ExcelLogDetail>()
            };

            int totalRows = 0;
            int successRows = 0;
            int errorRows = 0;
            var validRowPreview = new List<Dictionary<string, object>>();

            using var workbook = new XLWorkbook(excelStream);
            foreach (var worksheet in workbook.Worksheets)
            {
                var headerRow = worksheet.FirstRowUsed();
                if (headerRow == null) continue;

                var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var cell in headerRow.Cells())
                {
                    var colName = cell.GetString().Trim();
                    if (!string.IsNullOrEmpty(colName))
                        columnMap[colName] = cell.Address.ColumnNumber;
                }

                foreach (var col in columnMap.Keys)
                {
                    if (!mappedColumns.Contains(col))
                    {
                        excelLog.LogDetails.Add(new ExcelLogDetail
                        {
                            ExcelColumnName = col,
                            ErrorMessage = $"Colonna '{col}' non mappata nel sistema.",
                            RowNumber = null
                        });
                    }
                }

                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    totalRows++;
                    bool mappingOrConversionError = false;
                    var rowDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                    // Creazione entità
                    object entity;
                    if (entityType == 'C')
                        entity = new Cliente();
                    else if (entityType == 'P')
                        entity = new Prodotto();
                    else
                        throw new ArgumentException($"EntityType '{entityType}' non supportato.");

                    foreach (var map in details)
                    {
                        if (!columnMap.TryGetValue(map.ExcelColumnName, out int colIndex)) continue;

                        var cell = row.Cell(colIndex);
                        string rawValue = cell.GetString().Trim();

                        object? convertedValue;
                        try
                        {
                            convertedValue = ConvertValueByPropertyType(rawValue, GetNestedPropertyType(entity, map.EntityColumnName));
                            rowDict[map.ExcelColumnName] = convertedValue;
                        }
                        catch (Exception ex)
                        {
                            mappingOrConversionError = true;
                            excelLog.LogDetails.Add(new ExcelLogDetail
                            {
                                ExcelColumnName = map.ExcelColumnName,
                                ErrorMessage = $"Errore conversione '{rawValue}': {ex.Message}",
                                RowNumber = row.RowNumber()
                            });
                            continue;
                        }

                        try
                        {
                            SetNestedPropertyValue(entity, map.EntityColumnName, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            mappingOrConversionError = true;
                            excelLog.LogDetails.Add(new ExcelLogDetail
                            {
                                ExcelColumnName = map.ExcelColumnName,
                                ErrorMessage = $"Errore assegnazione '{map.ExcelColumnName}' a proprietà '{map.EntityColumnName}': {ex.Message}",
                                RowNumber = row.RowNumber()
                            });
                        }
                    }

                    if (mappingOrConversionError)
                    {
                        errorRows++;
                        continue;
                    }

                    var context = new ValidationContext(entity);
                    var results = new List<ValidationResult>();
                    bool isValid = Validator.TryValidateObject(entity, context, results, true);

                    if (!isValid)
                    {
                        foreach (var vr in results)
                        {
                            var member = vr.MemberNames.FirstOrDefault() ?? string.Empty;
                            var colName = details
                                .FirstOrDefault(d => d.EntityColumnName.Equals(member, StringComparison.OrdinalIgnoreCase))
                                ?.ExcelColumnName ?? member;

                            excelLog.LogDetails.Add(new ExcelLogDetail
                            {
                                ExcelColumnName = colName,
                                ErrorMessage = $"Validazione fallita per '{member}': {vr.ErrorMessage}",
                                RowNumber = row.RowNumber()
                            });
                        }
                        errorRows++;
                        continue;
                    }

                    try
                    {
                        if (entityType == 'C')
                        {
                            _dbContext.Clienti.Add((Cliente)entity);
                        }
                        else _dbContext.Prodotti.Add((Prodotto)entity);

                        await _dbContext.SaveChangesAsync();
                        successRows++;
                        validRowPreview.Add(rowDict);
                    }
                    catch (DbUpdateException dbEx)
                    {
                        _dbContext.Entry(entity).State = EntityState.Detached;

                        // Controlla se è errore di email duplicata
                        if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("IX_Clienti_Email"))
                        {
                            excelLog.LogDetails.Add(new ExcelLogDetail
                            {
                                ExcelColumnName = "Email",
                                ErrorMessage = $"Email duplicata non processata",
                                RowNumber = row.RowNumber()
                            });
                        }
                        else throw;

                        errorRows++;
                        continue;
                    }
                }

            }

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Errore durante il salvataggio delle entità: " + ex.Message, ex);
            }

            excelLog.TotalRows = totalRows;
            excelLog.SuccessRows = successRows;
            excelLog.ErrorRows = errorRows;
            _dbContext.ExcelLogs.Add(excelLog);
            await _dbContext.SaveChangesAsync();

            return new ImportExcelResponseDto
            {
                ExcelLogId = excelLog.Id,
                TotalRows = totalRows,
                SuccessRows = successRows,
                ErrorRows = errorRows,
                PreviewValidRows = validRowPreview
            };
        }

        private static object? ConvertValueByPropertyType(string rawValue, Type propertyType)
        {
            if (string.IsNullOrWhiteSpace(rawValue)) return null;
            var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (targetType == typeof(string)) return rawValue;
            if (targetType == typeof(int)) return int.Parse(rawValue);
            if (targetType == typeof(decimal)) return decimal.Parse(rawValue);
            if (targetType == typeof(DateTime)) return DateTime.Parse(rawValue);
            if (targetType == typeof(bool)) return bool.Parse(rawValue);

            return Convert.ChangeType(rawValue, targetType);
        }

        private static Type GetNestedPropertyType(object obj, string propertyPath)
        {
            var props = propertyPath.Split('.');
            Type currentType = obj.GetType();
            foreach (var prop in props)
            {
                var propertyInfo = currentType.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                    throw new ArgumentException($"Proprietà '{prop}' non trovata su '{currentType.Name}'");

                currentType = propertyInfo.PropertyType;
            }
            return currentType;
        }

        private static void SetNestedPropertyValue(object obj, string propertyPath, object? value)
        {
            var props = propertyPath.Split('.');
            object currentObject = obj;
            for (int i = 0; i < props.Length; i++)
            {
                var prop = currentObject.GetType().GetProperty(props[i], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) throw new ArgumentException($"Proprietà '{props[i]}' non trovata.");

                if (i == props.Length - 1)
                {
                    prop.SetValue(currentObject, value);
                }
                else
                {
                    var nextObject = prop.GetValue(currentObject);
                    if (nextObject == null)
                    {
                        nextObject = Activator.CreateInstance(prop.PropertyType);
                        prop.SetValue(currentObject, nextObject);
                    }
                    currentObject = nextObject!;
                }
            }
        }
    }
}
