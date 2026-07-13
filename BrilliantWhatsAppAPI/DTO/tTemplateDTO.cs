namespace BrilliantWhatsAppAPI.DTO;

public class tTemplateDTO
{
    public string TemplateName { get; set; }
    public string PhoneNumber { get; set; }
    public string LanguageCode { get; set; } = "en_US";
    public IList<tTemplateParameter> ParameterList { get; set; }
}

public class tTemplateParameter
{
    public string Type { get; set; }
    public string Text { get; set; }

}
