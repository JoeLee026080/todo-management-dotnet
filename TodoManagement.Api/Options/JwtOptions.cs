namespace TodoManagement.Api.Options;

public class JwtOptions
{
	public const string SectionName = "Jwt";

	public string SecretKey { get; set; } = string.Empty;

	public string Issuer { get; set; } = string.Empty;

	public string Audience { get; set; } = string.Empty;

	public int ExpiresInMinutes { get; set; }
}