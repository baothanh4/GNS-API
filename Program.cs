using API.Config;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add configuration file (appsettings.json) in current directory
builder.Configuration.SetBasePath(AppContext.BaseDirectory);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Configure MongoDB Settings
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

// Register MongoDB Services
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<PlayerProfileService>();
builder.Services.AddSingleton<InventoryService>();
builder.Services.AddSingleton<GameScoreService>();
builder.Services.AddSingleton<RoomService>();

// Add Controllers and JSON formatting
builder.Services.AddControllers();

// Configure CORS for Unity WebGL / local connection flexibility
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JWT Authentication
var secretKey = builder.Configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForNightShiftAsylumGame2026Project";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "NightShiftAsylumBackend",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "NightShiftAsylumClient",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowAll");

// Use Routing & Authentication
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configure the backend to run on port 5000 as configured in Unity APIManager.cs
app.Run("http://localhost:5000");
