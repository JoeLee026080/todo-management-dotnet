using Microsoft.AspNetCore.Mvc;
using TodoManagement.Api.Models;
using TodoManagement.Api.Services;

namespace TodoManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
	private const string AdminUsername = "admin";
	private const string AdminPassword = "admin123";
	private readonly JwtTokenService _jwtTokenService;

	public AuthController(JwtTokenService jwtTokenService)
	{
		_jwtTokenService = jwtTokenService;
	}

	[HttpPost("login")]
	public IActionResult Login([FromBody] LoginRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
		{
			return BadRequest(new { error = "請提供帳號（username）與密碼（password）" });
		}

		if (request.Username != AdminUsername || request.Password != AdminPassword)
		{
			return Unauthorized(new { error = "帳號或密碼錯誤" });
		}

		var token = _jwtTokenService.GenerateToken(request.Username);

		return Ok(new
		{
			token
		});
	}
}