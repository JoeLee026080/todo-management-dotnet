using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using TodoManagement.Api.Options;
using TodoManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<MongoDbOptions>(
	builder.Configuration.GetSection(MongoDbOptions.SectionName));
builder.Services.Configure<JwtOptions>(
	builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateIssuerSigningKey = true,
			ValidateLifetime = true,
			ValidIssuer = jwtOptions.Issuer,
			ValidAudience = jwtOptions.Audience,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
			ClockSkew = TimeSpan.Zero
		};
	});
builder.Services.AddAuthorization();
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
builder.Services.AddSingleton<JwtTokenService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
