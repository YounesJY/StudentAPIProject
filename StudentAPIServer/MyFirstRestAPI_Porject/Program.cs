using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StudentApi.Configuration;
using StudentDataAccessLayer;
using System.Text;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
DataAccesLayerSettings.SetConnectionString(
    builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("ConnectionStrings:Default is missing."));

var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSection.Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings section is missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
    throw new InvalidOperationException("JwtSettings:SecretKey is missing.");

builder.Services.Configure<JwtSettings>(jwtSection);
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("StudentApiCorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7062",
                "http://localhost:5151"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ===============================
// JWT Authentication Configuration
// ===============================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// ===============================
// Swagger Configuration
// ===============================
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("StudentApiCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
