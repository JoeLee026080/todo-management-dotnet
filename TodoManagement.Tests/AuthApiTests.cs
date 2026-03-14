using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace TodoManagement.Tests;

public class AuthApiTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly CustomWebApplicationFactory _factory;

	public AuthApiTests(CustomWebApplicationFactory factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	public async Task InitializeAsync()
	{
		await _factory.ResetDatabaseAsync();
	}

	public Task DisposeAsync()
	{
		_client.DefaultRequestHeaders.Authorization = null;
		return Task.CompletedTask;
	}

	[Fact]
	public async Task 正確帳號密碼應可登入並取得Token()
	{
		var response = await _client.PostAsJsonAsync("/api/auth/login", new
		{
			username = "admin",
			password = "admin123"
		});

		using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		var root = json.RootElement;

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.True(root.TryGetProperty("token", out var tokenProperty));
		Assert.False(string.IsNullOrWhiteSpace(tokenProperty.GetString()));
	}

	[Fact]
	public async Task 帳號錯誤應回傳401()
	{
		var response = await _client.PostAsJsonAsync("/api/auth/login", new
		{
			username = "wrong-user",
			password = "admin123"
		});

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task 密碼錯誤應回傳401()
	{
		var response = await _client.PostAsJsonAsync("/api/auth/login", new
		{
			username = "admin",
			password = "wrong-password"
		});

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task 缺少帳號應回傳400()
	{
		var response = await _client.PostAsJsonAsync("/api/auth/login", new
		{
			password = "admin123"
		});

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task 缺少密碼應回傳400()
	{
		var response = await _client.PostAsJsonAsync("/api/auth/login", new
		{
			username = "admin"
		});

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task 未帶Token呼叫受保護路由應回傳401()
	{
		var response = await _client.GetAsync("/api/items");

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task 使用無效Token呼叫受保護路由應回傳401()
	{
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "this.is.invalid");

		var response = await _client.GetAsync("/api/items");

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task 使用有效Token呼叫受保護路由應成功()
	{
		var token = await LoginAsync();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		var response = await _client.GetAsync("/api/items");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	private async Task<string> LoginAsync()
	{
		var response = await _client.PostAsJsonAsync("/api/auth/login", new
		{
			username = "admin",
			password = "admin123"
		});

		response.EnsureSuccessStatusCode();

		using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		return json.RootElement.GetProperty("token").GetString() ?? string.Empty;
	}
}