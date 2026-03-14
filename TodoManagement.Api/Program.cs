using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TodoManagement.Api.Options;
using TodoManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<MongoDbOptions>(
	builder.Configuration.GetSection(MongoDbOptions.SectionName));
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
	var options = serviceProvider.GetRequiredService<IOptions<MongoDbOptions>>().Value;
	return new MongoClient(options.ConnectionString);
});
builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
	var options = serviceProvider.GetRequiredService<IOptions<MongoDbOptions>>().Value;
	var client = serviceProvider.GetRequiredService<IMongoClient>();
	return client.GetDatabase(options.DatabaseName);
});
builder.Services.AddSingleton<ItemRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
