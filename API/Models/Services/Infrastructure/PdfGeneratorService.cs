// Compatibile con QuestPDF Community Edition v2023.x
using API.Models.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace API.Models.Services.Infrastructure;

public class PdfGeneratorService : IPdfService
{
    private const string CompanyName = "EasyShop";
    private static readonly byte[] LogoBytes;
    private const string FooterInfo = "EasyShop S.r.l.";
    private static readonly CultureInfo PdfCulture = new CultureInfo("it-IT");
      
    static PdfGeneratorService()
    {
        LogoBytes = File.ReadAllBytes("wwwroot/logo.jpg");
    }

    public byte[] GenerateClientList(List<Cliente> clienti)
        => CreateDocument(page =>
        {
            ApplyStandardLayout(page);
            page.Content().Element(ctx => BuildCustomerTable(ctx, clienti));
        });

    public byte[] GenerateOrderReceipt(Ordine ordine)
        => CreateDocument(page =>
        {
            ApplyStandardLayout(page);
            page.Content().Element(ctx => BuildOrderContent(ctx, ordine));
        });

    public byte[] GenerateProductList(List<Prodotto> prodotti)
        => CreateDocument(page =>
        {
            ApplyStandardLayout(page);
            page.Content().Element(ctx => BuildProductTable(ctx, prodotti));
        });

    public byte[] GenerateSalesReport(List<Ordine> ordini, DateTime start, DateTime end)
        => CreateDocument(page =>
        {
            ApplyStandardLayout(page);
            page.Content().Element(ctx => BuildSalesTable(ctx, ordini, start, end));
        });

    private byte[] CreateDocument(Action<PageDescriptor> pageSetup)
    {
        var document = Document.Create(container =>
        {
            container.Page(pageSetup);
        });
        return document.GeneratePdf();
    }

    private void ApplyStandardLayout(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.PageColor(Colors.Grey.Lighten4);

        // Bordo laterale sinistro sottile
        page.Background().AlignLeft().Width(4).Background(Colors.Blue.Darken2);

        // Header elegante
        page.Header().Height(60).Background(Colors.White).PaddingHorizontal(10).Row(row =>
        {
            // Logo
            row.ConstantItem(60).PaddingVertical(5).Element(c =>
            {
                c.Image(LogoBytes, ImageScaling.FitArea);
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().AlignMiddle().AlignLeft().Text(CompanyName)
                    .FontSize(18).FontColor(Colors.Blue.Darken3).SemiBold();

                col.Item().AlignLeft().Text("support@easyshop.com | +39 000 000 0000")
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
            });

            row.ConstantItem(180).AlignMiddle().AlignRight().Column(col =>
            {
                col.Spacing(2);
                col.Item().Text("Via Inventata, 42").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text("Città Invisibile, CAP 00000").FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });

        // Contenuto
        page.Content().PaddingVertical(15).PaddingHorizontal(10);

        // Footer con ragione sociale e info pagina
        page.Footer().Height(45).Background(Colors.White).PaddingHorizontal(10).Row(row =>
        {
            row.RelativeItem().AlignMiddle().Text(txt =>
            {
                txt.Span("© 2025 EasyShop S.r.l. – P.IVA 12345678901")
                    .FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            row.ConstantItem(150).AlignMiddle().AlignRight().Text(x =>
            {
                x.Span("Pagina ").FontSize(8).FontColor(Colors.Grey.Darken1);
                x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken2);
                x.Span(" di ").FontSize(8).FontColor(Colors.Grey.Darken1);
                x.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken2);
            });
        });
    }


    [Obsolete]
    private void BuildOrderContent(IContainer ctx, Ordine ordine)
    {
        ctx.Column(column =>
        {
            column.Spacing(5);
            column.Item().Text($"Ricevuta Ordine #{ordine.IdOrdine}")
                  .FontSize(16).SemiBold().FontColor(Colors.Blue.Darken1);
            column.Item().Text($"Data: {ordine.DataOrdine:dd/MM/yyyy}")
                  .FontSize(12);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(cols => { cols.RelativeColumn(4); cols.RelativeColumn(2); cols.RelativeColumn(2); cols.RelativeColumn(2); });
                table.Header(h =>
                {
                    h.Cell().Text("Prodotto").SemiBold();
                    h.Cell().AlignRight().Text("Quantità").SemiBold();
                    h.Cell().AlignRight().Text("Prezzo Unitario").SemiBold();
                    h.Cell().AlignRight().Text("Totale").SemiBold();
                });
                foreach (var item in ordine.DettagliOrdini)
                {
                    table.Cell().Text(item.Prodotto.NomeProdotto).WrapAnywhere().FontSize(12);
                    table.Cell().AlignRight().Text(item.Quantita.ToString()).FontSize(12);
                    table.Cell().AlignRight().Text(item.Prodotto.Prezzo.ToString("C2", PdfCulture)).FontSize(12);
                    table.Cell().AlignRight().Text((item.Quantita * item.Prodotto.Prezzo).ToString("C2", PdfCulture)).FontSize(12);
                }
            });

            var total = ordine.DettagliOrdini.Sum(i => i.Quantita * i.Prodotto.Prezzo);
            column.Item().AlignRight().Text($"Totale: {total.ToString("C2", PdfCulture)}")
                  .FontSize(14).SemiBold();
        });
    }

    private void BuildProductTable(IContainer ctx, List<Prodotto> prodotti)
    {
        ctx.Column(column =>
        {
            column.Spacing(5);
            column.Item().Text("Anagrafica Prodotti").FontSize(16).SemiBold().Underline().FontColor(Colors.Blue.Darken1);
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(cols => { cols.RelativeColumn(1); cols.RelativeColumn(4); cols.RelativeColumn(2); });
                table.Header(h =>
                {
                    h.Cell().Text("ID").SemiBold();
                    h.Cell().Text("Nome").SemiBold();
                    h.Cell().AlignRight().Text("Prezzo").SemiBold();
                });
                int idx = 0;
                foreach (var p in prodotti)
                {
                    var bg = idx++ % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;
                    table.Cell().Background(bg).Text(p.IdProdotto.ToString()).FontSize(12);
                    table.Cell().Background(bg).Text(p.NomeProdotto).FontSize(12);
                    table.Cell().Background(bg).AlignRight().Text(p.Prezzo.ToString("C2", PdfCulture)).FontSize(12);
                }
            });
        });
    }

    private void BuildCustomerTable(IContainer ctx, List<Cliente> customers)
    {
        ctx.Column(column =>
        {
            column.Spacing(5);
            column.Item().Text("Anagrafica Clienti").FontSize(16).SemiBold().FontColor(Colors.Blue.Darken1);
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(cols => { cols.RelativeColumn(1); cols.RelativeColumn(4); cols.RelativeColumn(4); cols.RelativeColumn(3); cols.RelativeColumn(3); cols.RelativeColumn(3); });
                table.Header(h =>
                {
                    var bg = Colors.Blue.Darken1;
                    h.Cell().Background(bg).Padding(5).Text("ID").FontColor(Colors.White).SemiBold();
                    h.Cell().Background(bg).Padding(5).Text("Nome").FontColor(Colors.White).SemiBold();
                    h.Cell().Background(bg).Padding(5).Text("Email").FontColor(Colors.White).SemiBold();
                    h.Cell().Background(bg).Padding(5).Text("Telefono").FontColor(Colors.White).SemiBold();
                    h.Cell().Background(bg).Padding(5).Text("Data Iscrizione").FontColor(Colors.White).SemiBold();
                    h.Cell().Background(bg).Padding(5).Text("Indirizzo").FontColor(Colors.White).SemiBold();
                });
                foreach (var c in customers)
                {
                    var text = c.Indirizzo != null ? $"{c.Indirizzo.Via}, {c.Indirizzo.Citta} - {c.Indirizzo.CAP}" : "N/A";
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(c.Id.ToString()).FontSize(12);
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(c.Nome ?? string.Empty).FontSize(12);
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(c.Email ?? string.Empty).FontSize(12);
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(c.NumeroTelefono ?? string.Empty).FontSize(12);
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(c.DataIscrizione.ToString("dd/MM/yyyy", PdfCulture)).FontSize(12);
                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(text).FontSize(12);
                }
            });
        });
    }

    private void BuildSalesTable(IContainer ctx, List<Ordine> sales, DateTime from, DateTime to)
    {
        var totaleVendite = sales.Sum(o => o.TotaleOrdine);

        ctx.Column(column =>
        {
            column.Spacing(10);

            column.Item().Text($"📦 Report Vendite dal {from:dd/MM/yyyy} al {to:dd/MM/yyyy}")
                .FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);

            // Totale vendite del periodo
            column.Item().Text($"💰 Totale vendite dal {from:dd/MM/yyyy} al {to:dd/MM/yyyy}: {totaleVendite.ToString("C2", PdfCulture)}")
                .FontSize(14).Bold().FontColor(Colors.Green.Darken3);

            foreach (var ordine in sales)
            {
                column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(orderColumn =>
                {
                    orderColumn.Spacing(4);

                    // Riga intestazione ordine
                    orderColumn.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Ordine #{ordine.IdOrdine} - {ordine.DataOrdine:dd/MM/yyyy}")
                            .Bold().FontSize(13).FontColor(Colors.Black);
                        row.ConstantItem(200).AlignRight().Text($"Totale: {ordine.TotaleOrdine.ToString("C2", PdfCulture)}")
                            .FontSize(12).FontColor(Colors.Green.Darken3).SemiBold();
                    });

                    // Info cliente
                    var cliente = ordine.Cliente;
                    orderColumn.Item().Text($"👤 {cliente.Nome} {cliente.Cognome} - ✉️ {cliente.Email}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2);

                    // Metodo di pagamento e indirizzo
                    orderColumn.Item().Text($"💳 Pagamento: {ordine.MetodoPagamento}   📍 Spedizione: {ordine.IndirizzoSpedizione}")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);

                    // Tabella prodotti
                    orderColumn.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4); // Prodotto
                            cols.RelativeColumn(1); // Quantità
                            cols.RelativeColumn(2); // Prezzo unitario
                            cols.RelativeColumn(2); // Totale riga
                        });

                        // Header tabella
                        table.Header(header =>
                        {
                            header.Cell().Text("Prodotto").Bold().FontSize(10);
                            header.Cell().Text("Q.tà").Bold().FontSize(10);
                            header.Cell().Text("Prezzo unit.").Bold().FontSize(10);
                            header.Cell().Text("Totale").Bold().FontSize(10);
                        });

                        // Righe prodotti
                        foreach (var det in ordine.DettagliOrdini)
                        {
                            var prodotto = det.Prodotto;
                            table.Cell().Text(prodotto.NomeProdotto).FontSize(9);
                            table.Cell().Text(det.Quantita.ToString()).FontSize(9);
                            table.Cell().Text(det.Prodotto.Prezzo.ToString("C2", PdfCulture)).FontSize(9);
                            table.Cell().Text((det.Quantita * det.Prodotto.Prezzo).ToString("C2", PdfCulture)).FontSize(9);
                        }
                    });
                });
            }
        });
    }

}
