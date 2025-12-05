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

const string DataDir = "data";
const string LogsDir = "logs";

var builder = WebApplication.CreateBuilder(args);

// Dependency Injection

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
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
// builder.Services.AddScoped<LoggerService>();


builder.Services.AddOpenApi();

// Error handling / ProblemDetails
builder.Services.AddProblemDetails();

// Swagger (Correct - no NSwag)


var app = builder.Build();

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
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}
// identity
app.UseAuthentication();
app.UseAuthorization();

// Map your endpoints
app.MapAuthEndpoints();
app.MapOrderEndpoints();
app.MapUserEndpoints();

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
