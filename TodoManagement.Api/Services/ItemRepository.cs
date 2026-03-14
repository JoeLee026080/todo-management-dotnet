using Microsoft.Extensions.Options;
using MongoDB.Bson;
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

	public async Task<List<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await Collection.Find(FilterDefinition<TodoItem>.Empty).ToListAsync(cancellationToken);
	}

	public async Task<string> CreateAsync(TodoItem item, CancellationToken cancellationToken = default)
	{
		item.Id ??= ObjectId.GenerateNewId().ToString();
		await Collection.InsertOneAsync(item, cancellationToken: cancellationToken);
		return item.Id;
	}

	public async Task<UpdateResult> UpdateNameAsync(string id, string name, CancellationToken cancellationToken = default)
	{
		var filter = Builders<TodoItem>.Filter.Eq(item => item.Id, id);
		var update = Builders<TodoItem>.Update.Set(item => item.Name, name);
		return await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
	}

	public async Task<DeleteResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
	{
		var filter = Builders<TodoItem>.Filter.Eq(item => item.Id, id);
		return await Collection.DeleteOneAsync(filter, cancellationToken);
	}
}