using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace Frontend.Pages.Prodotti
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty]
        [Required(ErrorMessage = "Il nome è obbligatorio")]
        public string NomeProdotto { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La descrizione è obbligatoria")]
        [MaxLength(500, ErrorMessage = "La descrizione non può superare i 500 caratteri")]
        public string Descrizione { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Il prezzo è obbligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Il prezzo deve essere maggiore di zero")]
        public decimal Prezzo { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La categoria è obbligatoria")]
        public string Categoria { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La quantità è obbligatoria")]
        [Range(0, int.MaxValue, ErrorMessage = "La quantità deve essere un numero positivo")]
        public int Giacenza { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "L'immagine è obbligatoria")]
        public IFormFile ImgFile { get; set; }

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(NomeProdotto), "NomeProdotto" },
                    { new StringContent(Descrizione), "Descrizione" },
                    { new StringContent(Prezzo.ToString()), "Prezzo" },
                    { new StringContent(Categoria), "Categoria" },
                    { new StringContent(Giacenza.ToString()), "Giacenza" }
                };

                if (ImgFile != null && ImgFile.Length > 0)
                {
                    var fileContent = new StreamContent(ImgFile.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(ImgFile.ContentType);
                    formData.Add(fileContent, "ImgFile", ImgFile.FileName);
                }

                var token = Request.Cookies["jwtToken"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync("api/prodotti", formData); // BaseAddress è già configurato

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Errore API: {response.StatusCode} - {errorContent}");
                    return Page();
                }

                // Puoi decidere se reindirizzare o svuotare il form
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Errore critico: {ex.Message}");
                return Page();
            }
        }
    }
}
