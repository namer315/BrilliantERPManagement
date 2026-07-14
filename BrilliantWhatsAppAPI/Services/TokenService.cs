using System.Security.Cryptography;
using System.Text.Json;

namespace BrilliantWhatsAppAPI.Services;

public class TokenService
{
    // private const string TokenServiceFilePath = "ERPData\\TokenService.json";
    private static readonly string TokenServiceFilePath = 
                            Path.Combine(AppContext.BaseDirectory, "ERPData", "TokenService.json");
    //public static TokenServiceDTO _tokenService { get; set; } = new TokenServiceDTO();

    public static Tenant ValidateToken(string token)
    {
        TokenServiceDTO tokenService = GetTokenServiceFromFile();
        if (tokenService.TokenList.FirstOrDefault(t => t.Token == token) is Tenant tenant)
        {
            if(!tenant.Active)
                throw new UnauthorizedAccessException(
                $"Tenant '{tenant.Name}' is currently deactivated. Access denied.");

            return tenant;
        }

        throw new UnauthorizedAccessException(
        "Invalid API token: the provided token does not match any registered tenant.");
    }

    /// <summary>
    /// Loads TokenServiceDTO from a JSON file. Creates the file if it doesn't exist.
    /// </summary>
    public static TokenServiceDTO GetTokenServiceFromFile()
    {
        if (!File.Exists(TokenServiceFilePath))
        {
            return CreateTokenServiceFile();
        }

        try
        {
            var jsonContent = File.ReadAllText(TokenServiceFilePath);
            var tokenService = JsonSerializer.Deserialize<TokenServiceDTO>(jsonContent)
                ?? throw new InvalidOperationException("Failed to deserialize token service data");

            return tokenService;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON format in token service file: {TokenServiceFilePath}" , ex);
        }
    }

    /// <summary>
    /// Creates TokenServiceFile if it doesn't exist with three default tenant records
    /// </summary>
    public static TokenServiceDTO CreateTokenServiceFile()
    {
        try
        {
            var directory = Path.GetDirectoryName(TokenServiceFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create default TokenServiceDTO with three tenant records
            var defaultTokenService = new TokenServiceDTO()
            {
                TokenList = new List<Tenant>()
                {
                    new Tenant
                    {
                        Name = "Tenant1",
                        Token = GenerateApiToken(),
                        Active = true
                    },
                    new Tenant
                    {
                        Name = "Tenant2",
                        Token = GenerateApiToken(),
                        Active = true
                    },
                    new Tenant
                    {
                        Name = "Tenant3",
                        Token = GenerateApiToken(),
                        Active = true
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(defaultTokenService , new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(TokenServiceFilePath , jsonContent);

            return defaultTokenService;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create token service file at: {TokenServiceFilePath}" , ex);
        }
    }

    public static string GenerateApiToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        return "sk-" + Convert.ToBase64String(bytes)
            .Replace("+" , "")
            .Replace("/" , "")
            .Replace("=" , "");
    }

}


public class TokenServiceDTO
{
    public IList<Tenant> TokenList { set; get; }
}
public class Tenant
{
    public string Name { set; get; }
    public string Token { set; get; }

    public bool Active { set; get; } = false;

}