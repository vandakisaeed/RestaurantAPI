using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

using RestaurantAPI.Endpoints;
using RestaurantAPI.Services;
using Scalar.AspNetCore;
using RestaurantAPI.Infrastructure;
using RestaurantAPI.Models;
using RestaurantAPI.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
// Directives
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Diagnostics.HealthChecks;

const string DataDir = "data";
const string LogsDir = "logs";

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("logs/blog-api-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "BlogApi");
});

// Program.cs - Add health checks after service registrations
builder.Services.AddHealthChecks()
    //.AddDbContextCheck<ApplicationDbContext>() // Check database connectivity
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"))
    .AddCheck<CustomHealthCheck>("custom-check"); // Custom business logic check
// Register custom health check
builder.Services.AddScoped<CustomHealthCheck>();

// Dependency Injection

// Register the application's SQLite DB only when NOT running tests.
// Integration tests will replace this with an InMemory provider via TestHost.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}
//seed
builder.Services.AddScoped<DbSeeder>();

// identity
builder.Services.AddIdentityCore<User>()
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
//builder.Services.AddSingleton(sp => new StorageService(DataDir));
// builder.Services.AddSingleton<IOrderService, OrderService>();
// builder.Services.AddSingleton<IUserService, UserService>();
// builder.Services.AddSingleton(sp =>
//    new LoggerService(LogsDir, sp.GetRequiredService<IOrderService>())
// );
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReportService, ReportServiceMock>();
// Program.cs - Add after other service registrations
builder.Services.AddSingleton<IMetricsService, MetricsService>();
// builder.Services.AddScoped<LoggerService>();


builder.Services.AddOpenApi();

// Error handling / ProblemDetails
builder.Services.AddProblemDetails();

// Swagger (Correct - no NSwag)


var app = builder.Build();


// Add HTTP request logging middleware
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null ? LogEventLevel.Error : LogEventLevel.Information;
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown");
    };
});



// GLOBAL ERROR HANDLING
app.UseExceptionHandler();
app.UseStatusCodePages();


// --- RUN DB SEEDER ---

// Swagger + Scalar in dev mode
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Scalar API reference

    app.MapScalarApiReference();

    app.UseSwaggerUi(options =>
     {
         options.DocumentPath = "/openapi/v1.json";
     });

}

//run seed
// --- RUN DB SEEDER ---
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();

    if (!app.Environment.IsEnvironment("Testing"))
    {
        await seeder.SeedAsync();   // FIXED HERE!
    }
}
// identity
app.UseAuthentication();
app.UseAuthorization();

// Map your endpoints
app.MapAuthEndpoints();
app.MapOrderEndpoints();
app.MapUserEndpoints();
app.MapReportEndpoints();

app.MapHealthEndpoints();

// Test endpoint
app.MapGet("/", () =>
{
    try
    {
        return TypedResults.Ok("Hello World!");
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 400);
    }
})
.Produces<string>(200)
.ProducesProblem(400)
.ProducesProblem(500);

// Run app
app.Run();

public partial class Program;
