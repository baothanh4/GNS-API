using API.Chat;
using API.Config;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<GlobalChatSettings>(
    builder.Configuration.GetSection("GlobalChat"));

builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IPlayerProfileRepository, PlayerProfileRepository>();
builder.Services.AddSingleton<IInventoryRepository, InventoryRepository>();
builder.Services.AddSingleton<IRoomRepository, RoomRepository>();
builder.Services.AddSingleton<IGameScoreRepository, GameScoreRepository>();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<PlayerProfileService>();
builder.Services.AddSingleton<InventoryService>();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<GameScoreService>();

// [GNS301_Require] Hosted service chạy TCP chat bất đồng bộ cùng vòng đời Web API.
builder.Services.AddHostedService<GlobalChatServer>();

builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddPolicy("UnityClient", policy =>
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

string secret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret is missing.");
byte[] key = Encoding.UTF8.GetBytes(secret);

// [GNS301_Require] JWT bearer xác thực role Player/Admin trước khi vào endpoint riêng tư.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseCors("UnityClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

await app.RunAsync("http://0.0.0.0:5000");
