using System.Net;
using System.Text.Json;

using API.Models.DTO;
using API.Models.Entities;
using API.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _googleMapsApiKey;
        private readonly IConfiguration _config;
        public AddressController(HttpClient httpClient,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IConfiguration config)
        {
            _httpClient = httpClient;
            _userManager = userManager;
            _dbContext = dbContext;
            _config = config;
            _googleMapsApiKey = config["GoogleMaps:ApiKey"] ?? string.Empty;
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetAddress(
            [FromQuery] string query,
            [FromQuery] string lang = "en",
            [FromQuery] int limit = 5
        )
        {
            try
            {
                var sanitizedQuery = WebUtility.UrlEncode(query);
                var url = $"https://photon.komoot.io/api/?q={sanitizedQuery}&lang={lang}&limit={limit}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");

            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("is-valid-address")]
        public async Task<IActionResult> IsValidAddress(
            [FromQuery] string city = "",
            [FromQuery] string street = "",
            [FromQuery] string postalcode = "",
            [FromQuery] string housenumber = "")
        {
            try
            {
                if (string.IsNullOrEmpty(city)
                    || string.IsNullOrEmpty(street)
                    || string.IsNullOrEmpty(postalcode)
                    || string.IsNullOrEmpty(housenumber))
                {
                    return BadRequest(new { error = "I campi non possono essere vuoti." });
                }

                var sanitizedQuery = WebUtility.UrlEncode($"{street} {city} {postalcode} {housenumber}");
                var url = $"https://photon.komoot.io/api/?q={sanitizedQuery}&lang=en&limit=5";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AddressResponse>(content);

                bool isValid = false;
                string validCity = string.Empty;
                string validPostalCode = string.Empty;
                string validName = string.Empty;
                string validHouseNumber = string.Empty;

                if (result?.Features != null)
                {
                    foreach (var feature in result.Features)
                    {
                        var props = feature.PropertiesProp;
                        // Confronta i valori in modo case-insensitive e controlla la corrispondenza
                        if (props.City.Equals(city, StringComparison.OrdinalIgnoreCase) &&
                            props.Postalcode.Equals(postalcode, StringComparison.OrdinalIgnoreCase) &&
                            props.Name.Contains(street, StringComparison.OrdinalIgnoreCase) &&
                            props.Housenumber.Equals(housenumber, StringComparison.OrdinalIgnoreCase))
                        {
                            isValid = true;
                            validCity = props.City;
                            validPostalCode = props.Postalcode;
                            validName = props.Name;
                            validHouseNumber = props.Housenumber;
                            break;
                        }
                    }
                }

                return Ok(new
                {
                    isValid = isValid,
                    address = new
                    {
                        city = validCity ?? "",
                        postalcode = validPostalCode ?? "",
                        name = validName ?? "",
                        housenumber = validHouseNumber ?? ""
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("get-user-address")]
        [Authorize]
        public async Task<IActionResult> GetUserAddress()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Nessun utente trovato");
            string userId = user.Id;

            var cliente = await _dbContext.Clienti.FirstOrDefaultAsync(c => c.UserId == userId);

            return Ok(new UserAddressResponseDto
            {
                Indirizzo_Citta = cliente?.Indirizzo?.Citta ?? string.Empty,
                Indirizzo_CAP = cliente?.Indirizzo?.CAP ?? string.Empty,
                Indirizzo_Via = cliente?.Indirizzo?.Via ?? string.Empty,
                Indirizzo_HouseNumber = cliente?.Indirizzo?.HouseNumber ?? string.Empty

            });

        }

        [HttpGet("google-places-address")]
        public async Task<IActionResult> GetGooglePlacesAddress([FromQuery] string query, [FromQuery] string lang = "it")
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { error = "Query cannot be empty." });

            var autocompleteUrl = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={query}&language={lang}&key={_googleMapsApiKey}";

            try
            {
                var autocompleteResponse = await _httpClient.GetAsync(autocompleteUrl);
                autocompleteResponse.EnsureSuccessStatusCode();

                var autocompleteContent = await autocompleteResponse.Content.ReadAsStringAsync();
                var autocompleteJson = JsonDocument.Parse(autocompleteContent);

                var predictions = autocompleteJson.RootElement.GetProperty("predictions");
                if (predictions.GetArrayLength() == 0)
                    return NotFound(new { error = "Nessun indirizzo trovato." });

                var results = new List<object>();

                foreach (var prediction in predictions.EnumerateArray())
                {
                    var placeId = prediction.GetProperty("place_id").GetString();
                    var description = prediction.GetProperty("description").GetString();

                    var detailsUrl = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&language={lang}&key={_googleMapsApiKey}";
                    var detailsResponse = await _httpClient.GetAsync(detailsUrl);
                    detailsResponse.EnsureSuccessStatusCode();

                    var detailsContent = await detailsResponse.Content.ReadAsStringAsync();
                    var detailsJson = JsonDocument.Parse(detailsContent);

                    var result = detailsJson.RootElement.GetProperty("result");
                    var addressComponents = result.GetProperty("address_components");

                    string? street = null, houseNumber = null, city = null, postalCode = null, province = null, country = null;

                    foreach (var component in addressComponents.EnumerateArray())
                    {
                        var types = component.GetProperty("types").EnumerateArray().Select(t => t.GetString()).ToList();
                        var longName = component.GetProperty("long_name").GetString();

                        if (types.Contains("route"))
                            street = longName;
                        else if (types.Contains("street_number"))
                            houseNumber = longName;
                        else if (types.Contains("locality"))
                            city = longName;
                        else if (types.Contains("postal_code"))
                            postalCode = longName;
                        else if (types.Contains("administrative_area_level_2"))
                            province = longName;
                        else if (types.Contains("country"))
                            country = longName;
                    }

                    results.Add(new
                    {
                        FullAddress = description,
                        Street = street,
                        HouseNumber = houseNumber,
                        City = city,
                        PostalCode = postalCode,
                        Province = province,
                        Country = country
                    });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }

    public class AddressResponse
    {
        public List<Feature> Features { get; set; } = new();

        public class Feature
        {
            public string Type { get; set; } = string.Empty;
            [JsonProperty("properties")]
            public Properties PropertiesProp { get; set; } = new();
            [JsonProperty("geometry")]
            public Geometry GeometryProp { get; set; } = new();

            public class Geometry
            {
                public string Type { get; set; } = string.Empty;
                public List<double> Coordinates { get; set; } = new();
            }

            public class Properties
            {
                [JsonProperty("name")]
                public string Name { get; set; } = string.Empty;
                [JsonProperty("city")]
                public string City { get; set; } = string.Empty;
                [JsonProperty("postcode")]
                public string Postalcode { get; set; } = string.Empty;
                [JsonProperty("housenumber")]
                public string Housenumber { get; set; } = string.Empty;
            }
        }
    }
}
