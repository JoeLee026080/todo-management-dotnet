using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Mongo2Go;
using MongoDB.Driver;
using TodoManagement.Api.Models;

namespace TodoManagement.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
	private readonly MongoDbRunner _mongoRunner;
	private readonly string _databaseName;

	public CustomWebApplicationFactory()
	{
		_mongoRunner = MongoDbRunner.Start(singleNodeReplSet: true);
		_databaseName = $"todo_management_tests_{Guid.NewGuid():N}";
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration((_, configBuilder) =>
		{
			configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["MongoDb:ConnectionString"] = _mongoRunner.ConnectionString,
				["MongoDb:DatabaseName"] = _databaseName,
				["MongoDb:ItemsCollectionName"] = "items",
				["Jwt:SecretKey"] = "development-secret-key-change-me",
				["Jwt:Issuer"] = "TodoManagement.Api",
				["Jwt:Audience"] = "TodoManagement.Client",
				["Jwt:ExpiresInMinutes"] = "60"
			});
		});
	}

	public async Task ResetDatabaseAsync()
	{
		var client = new MongoClient(_mongoRunner.ConnectionString);
		await client.DropDatabaseAsync(_databaseName);
	}

	public async Task SeedItemsAsync(params TodoItem[] items)
	{
		if (items.Length == 0)
		{
			return;
		}

		var client = new MongoClient(_mongoRunner.ConnectionString);
		var database = client.GetDatabase(_databaseName);
		var collection = database.GetCollection<TodoItem>("items");
		await collection.InsertManyAsync(items);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		if (disposing)
		{
			_mongoRunner.Dispose();
		}
	}
}