using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Frontend.Customizations.TagHelpers;

[HtmlTargetElement("validation-errors", Attributes = "errors")]
public class ValidationErrorsTagHelper : TagHelper
{
    [HtmlAttributeName("errors")]
    public List<string>? Errors { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Errors == null || Errors.Count == 0)
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = "div";
        output.Attributes.SetAttribute("class", "mb-4 p-4 bg-red-100 border border-red-400 rounded-lg shadow-md");

        var content = $@"
                <div class='flex items-center mb-2'>
                    <span><i class='fas fa-exclamation-triangle text-red-600'></i></span>
                    <p class='text-red-600 font-semibold text-sm'>Si è verificato un errore nell'invio dei dati</p>
                </div>
                <ul class='list-disc pl-5 text-red-700 text-xs space-y-1'>
            ";

        foreach (var error in Errors)
        {
            content += $"<li>{error}</li>";
        }

        content += "</ul>";

        output.Content.SetHtmlContent(content);
    }
}
