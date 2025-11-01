namespace MovieRev.Core.Settings;

public class JwtSettings
{
    public const string SectionName = "Jwt"; // Имя секции в appsettings.json
    
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpireDays { get; set; }
    
}