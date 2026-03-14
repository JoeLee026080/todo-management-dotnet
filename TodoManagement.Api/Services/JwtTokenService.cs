using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TodoManagement.Api.Options;

namespace TodoManagement.Api.Services;

public class JwtTokenService
{
	private readonly JwtOptions _options;

	public JwtTokenService(IOptions<JwtOptions> options)
	{
		_options = options.Value;
	}

	public string GenerateToken(string username)
	{
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, username),
			new Claim(ClaimTypes.Role, "admin")
		};

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
		var token = new JwtSecurityToken(
			issuer: _options.Issuer,
			audience: _options.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(_options.ExpiresInMinutes),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}