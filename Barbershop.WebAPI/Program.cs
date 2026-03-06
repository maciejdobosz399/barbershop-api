using Asp.Versioning.ApiExplorer;
using Azure.Identity;
using Barbershop.Application;
using Barbershop.Infrastructure;
using Barbershop.WebAPI;
using Barbershop.WebAPI.Middleware;
using Barbershop.WebAPI.Services;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("AzureAppConfiguration");

builder.Configuration.AddAzureAppConfiguration(options =>
{
	options.Connect(connectionString)
		   .Select(KeyFilter.Any, LabelFilter.Null)
		   .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
		   .ConfigureKeyVault(kv =>
		   {
			   kv.SetCredential(new DefaultAzureCredential());
		   })
		   .ConfigureRefresh(refresh =>
		   {
			   refresh.Register("SentinelKey", refreshAll: true)
					  .SetRefreshInterval(TimeSpan.FromSeconds(30));
		   });
});

builder.Services.AddOpenApi();

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var connstring = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddApplication()
	.AddInfrastructure(builder.Configuration)
	.AddWebAPI(builder.Configuration)
	.AddVersioningAndSwagger();

builder.Services.AddHostedService<DatabaseInitializerService>();

builder.Host.UseWolverine(options =>
{
	options.Durability.Mode = DurabilityMode.MediatorOnly;
	options.Discovery.IncludeAssembly(typeof(DependancyInjection).Assembly);
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8081";
builder.WebHost.ConfigureKestrel(options =>
{
	options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

//if (app.Environment.IsDevelopment())
//{
	app.MapOpenApi();
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		foreach (var description in app.Services.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions)
		{
			c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
		}
	});
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
