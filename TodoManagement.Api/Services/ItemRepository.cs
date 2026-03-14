using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TodoManagement.Api.Models;
using TodoManagement.Api.Options;

namespace TodoManagement.Api.Services;

public class ItemRepository
{
	public ItemRepository(IMongoDatabase database, IOptions<MongoDbOptions> options)
	{
		var mongoDbOptions = options.Value;
		Collection = database.GetCollection<TodoItem>(mongoDbOptions.ItemsCollectionName);
	}

	public IMongoCollection<TodoItem> Collection { get; }
}