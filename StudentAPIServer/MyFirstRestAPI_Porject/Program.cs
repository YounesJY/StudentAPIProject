using StudentDataAccessLayer;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
DataAccesLayerSettings.SetConnectionString(
    builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("ConnectionStrings:Default is missing."));
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
app.UseAuthorization();
app.MapControllers();

app.Run();
