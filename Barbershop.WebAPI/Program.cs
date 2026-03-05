using Asp.Versioning.ApiExplorer;
using Azure.Identity;
using Barbershop.Application;
using Barbershop.Infrastructure;
using Barbershop.Infrastructure.DbContexts;
using Barbershop.WebAPI;
using Barbershop.WebAPI.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
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

builder.Host.UseWolverine(options =>
{
    options.Durability.Mode = DurabilityMode.MediatorOnly;
    options.Discovery.IncludeAssembly(typeof(DependancyInjection).Assembly);
});

var app = builder.Build();

if (app.Environment.IsProduction())
{
    using var migrationScope = app.Services.CreateScope();
    var dbContext = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    string[] roles = ["Admin"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
        }
    }
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        foreach (var description in app.Services.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
