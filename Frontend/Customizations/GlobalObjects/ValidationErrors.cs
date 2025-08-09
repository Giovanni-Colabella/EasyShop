using Newtonsoft.Json;

namespace Frontend.Customizations.GlobalObjects;

public class ValidationErrors
{
    [JsonProperty("errors")]
    public List<string> Errors { get; set; } = new();
}
