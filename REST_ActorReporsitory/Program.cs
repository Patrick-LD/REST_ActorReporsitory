using Microsoft.EntityFrameworkCore;
using REST_ActorReporsitory.Models;
using REST_ActorReporsitory.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Henter JWT-indstillinger fra appsettings.json under "Jwt" sektionen
var jwtSettings = builder.Configuration.GetSection("Jwt");
// Konverterer den hemmelige nøgle til bytes - bruges til at verificere indkommende tokens
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

// Tilføj services til containeren.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        // AllowAnyOrigin tillader alle domæner at kalde API'en
        // WithOrigins("http://localhost:3000") ville begrænse til specifikke domæner
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger konfiguration med JWT-understøttelse
// Dette tilføjer "Authorize" knappen i Swagger UI
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Definerer hvordan Bearer-token skal sendes (i Authorization headeren)
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token as: Bearer {your_token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    // Kræver at alle endpoints i Swagger bruger Bearer-token definitionen ovenfor
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddSingleton<IActorRepository>(new ActorRepositoryList(includeData: true));

// Konfigurerer authentication - fortæller ASP.NET at vi bruger JWT Bearer tokens
builder.Services.AddAuthentication(options =>
{
    // Sæt JWT Bearer som standard authentication-metode
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // Når en bruger afvises, send et "Bearer" challenge (dvs. "du mangler et token")
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Regler for hvordan indkommende tokens skal valideres
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,            // Tjek at token er fra den rigtige udsteder
        ValidateAudience = true,          // Tjek at token er til den rigtige modtager
        ValidateLifetime = true,          // Tjek at token ikke er udløbet
        ValidateIssuerSigningKey = true,  // Tjek at token er signeret med den rigtige nøgle
        ValidIssuer = jwtSettings["Issuer"],           // Forventet udsteder (fra appsettings.json)
        ValidAudience = jwtSettings["Audience"],       // Forventet modtager (fra appsettings.json)
        IssuerSigningKey = new SymmetricSecurityKey(key) // Nøglen der bruges til at verificere signaturen
    };
});

// Aktiverer [Authorize] attributten på controllers/endpoints
builder.Services.AddAuthorization();


// Brug environment variable hvis den findes, ellers brug appsettings
var connectionString = Environment.GetEnvironmentVariable("ActorDbConnection")
    ?? builder.Configuration.GetConnectionString("ActorDb");
builder.Services.AddDbContext<ActorDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
// VIGTIGT: UseAuthentication SKAL komme FØR UseAuthorization
// UseAuthentication læser og validerer tokenet fra Authorization-headeren
app.UseAuthentication();
// UseAuthorization tjekker om brugeren har adgang til det ønskede endpoint ([Authorize])
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/api/actors"));
app.MapControllers();

app.Run();
