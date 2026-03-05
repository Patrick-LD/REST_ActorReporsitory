using Microsoft.EntityFrameworkCore;
using REST_ActorReporsitory.Models;
using REST_ActorReporsitory.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {/*Hvis jeg skriver WithOrigins istedet for AllowAnyOrigin kan jeg s�tte
     links ind i (), s� kun de links kan tilg� API'en */
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IActorRepository>(new ActorRepositoryList(includeData: true));

// Brug environment variable hvis den findes, ellers brug appsettings
var connectionString = Environment.GetEnvironmentVariable("ActorDbConnection")
    ?? builder.Configuration.GetConnectionString("ActorDb");
builder.Services.AddDbContext<ActorDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/api/actors"));
app.MapControllers();

app.Run();
