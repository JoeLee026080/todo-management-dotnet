using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using TodoManagement.Api.Options;
using TodoManagement.Api.Services;

LoadDotEnvFile();

var builder = WebApplication.CreateBuilder(args);

ApplyLegacyEnvironmentOverrides(builder.Configuration);

builder.Services.AddControllers();
builder.Services.Configure<MongoDbOptions>(
	builder.Configuration.GetSection(MongoDbOptions.SectionName));
builder.Services.Configure<JwtOptions>(
	builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
	.Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
	{
		var jwtOptions = jwtOptionsAccessor.Value;
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

void ApplyLegacyEnvironmentOverrides(ConfigurationManager configuration)
{
	var overrides = new Dictionary<string, string?>();
	var mongoDbUri = Environment.GetEnvironmentVariable("MONGODB_URI");
	var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");

	if (!string.IsNullOrWhiteSpace(mongoDbUri))
	{
		overrides[$"{MongoDbOptions.SectionName}:ConnectionString"] = mongoDbUri;
	}

	if (!string.IsNullOrWhiteSpace(jwtSecret))
	{
		overrides[$"{JwtOptions.SectionName}:SecretKey"] = jwtSecret;
	}

	if (overrides.Count > 0)
	{
		configuration.AddInMemoryCollection(overrides);
	}
}

void LoadDotEnvFile()
{
	var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

	while (currentDirectory is not null)
	{
		var dotEnvPath = Path.Combine(currentDirectory.FullName, ".env");

		if (File.Exists(dotEnvPath))
		{
			foreach (var rawLine in File.ReadAllLines(dotEnvPath))
			{
				var line = rawLine.Trim();

				if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
				{
					continue;
				}

				var separatorIndex = line.IndexOf('=');

				if (separatorIndex <= 0)
				{
					continue;
				}

				var key = line[..separatorIndex].Trim();
				var value = line[(separatorIndex + 1)..].Trim().Trim('"');

				if (string.IsNullOrWhiteSpace(key) || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
				{
					continue;
				}

				Environment.SetEnvironmentVariable(key, value);
			}

			return;
		}

		currentDirectory = currentDirectory.Parent;
	}
}

public partial class Program;
