using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TodoManagement.Api.Models;

namespace TodoManagement.Tests;

public class ItemsApiTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly CustomWebApplicationFactory _factory;
	private string _authToken = string.Empty;

	public ItemsApiTests(CustomWebApplicationFactory factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	public async Task InitializeAsync()
	{
		await _factory.ResetDatabaseAsync();
		_authToken = await LoginAsync();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
	}

	public Task DisposeAsync()
	{
		_client.DefaultRequestHeaders.Authorization = null;
		return Task.CompletedTask;
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

	[Fact]
	public async Task 取得所有項目應回傳已存在的資料()
	{
		await _factory.SeedItemsAsync(
			new TodoItem { Name = "Test Item 1" },
			new TodoItem { Name = "Test Item 2" });

		var response = await _client.GetAsync("/api/items");
		var items = await response.Content.ReadFromJsonAsync<List<TodoItem>>();

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.NotNull(items);
		Assert.Equal(2, items.Count);
	}

	[Fact]
	public async Task 新增項目應回傳成功與新增識別碼()
	{
		var response = await _client.PostAsJsonAsync("/api/items", new TodoItem
		{
			Name = "New Test Item"
		});

		using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		var root = json.RootElement;

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.True(root.GetProperty("acknowledged").GetBoolean());
		Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("insertedId").GetString()));
	}

	[Fact]
	public async Task 更新項目應回傳成功結果()
	{
		var item = new TodoItem { Id = Guid.NewGuid().ToString("N")[..24], Name = "Old Name" };
		await _factory.SeedItemsAsync(item);

		var response = await _client.PutAsJsonAsync($"/api/items/{item.Id}", new TodoItem
		{
			Name = "Updated Name"
		});

		using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		var root = json.RootElement;

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.True(root.GetProperty("acknowledged").GetBoolean());
		Assert.Equal(1, root.GetProperty("matchedCount").GetInt64());
		Assert.Equal(1, root.GetProperty("modifiedCount").GetInt64());
	}

	[Fact]
	public async Task 刪除項目應回傳成功結果()
	{
		var item = new TodoItem { Id = Guid.NewGuid().ToString("N")[..24], Name = "Delete Me" };
		await _factory.SeedItemsAsync(item);

		var response = await _client.DeleteAsync($"/api/items/{item.Id}");

		using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		var root = json.RootElement;

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.True(root.GetProperty("acknowledged").GetBoolean());
		Assert.Equal(1, root.GetProperty("deletedCount").GetInt64());
	}
}