using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace REST_ActorReporsitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] på klasse-niveau betyder at ALLE endpoints i denne controller kræver token.
    // Enkelte metoder kan overskrive dette med [AllowAnonymous]
    [Authorize]
    public class AuthController : ControllerBase
    {
        // IConfiguration giver adgang til appsettings.json (f.eks. JWT nøgle, issuer, audience)
        private readonly IConfiguration _config;

        // Constructor injection - ASP.NET giver os automatisk IConfiguration
        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        // [AllowAnonymous] overskriver [Authorize] på klassen, så denne metode kan kaldes UDEN token.
        // Dette er nødvendigt - ellers kan ingen logge ind, da man skal have token for at få token
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            // I et rigtigt system ville man tjekke mod en database
            if (login.Username == "admin" && login.Password == "1234")
            {
                var token = GenerateJwtToken(login.Username);
                // Returnerer token som JSON: { "token": "eyJ..." }
                return Ok(new { token });
            }

            // 401 Unauthorized - forkert brugernavn eller password
            return Unauthorized("Invalid username or password.");
        }

        private string GenerateJwtToken(string username)
        {
            // Henter JWT-indstillinger fra "Jwt" sektionen i appsettings.json
            var jwtSettings = _config.GetSection("Jwt");

            // Opretter en symmetrisk nøgle fra den hemmelige streng - bruges til at signere tokenet
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            // SigningCredentials kombinerer nøglen med algoritmen (HMAC-SHA256)
            // Dette er "underskriften" der beviser at tokenet er ægte
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims er informationer der bliver pakket ind i tokenet.
            // Serveren kan læse disse uden at slå op i en database
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),   // Subject: hvem tokenet tilhører
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unikt ID pr. token
                new Claim(ClaimTypes.Name, username),               // Brugernavn
                new Claim(ClaimTypes.Role, "Admin")                 // Rolle - kan bruges til rollebaseret adgang
            };

            // Samler alle dele til ét JWT token
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],       // Hvem udstedte tokenet (skal matche Program.cs)
                audience: jwtSettings["Audience"],   // Hvem er tokenet beregnet til (skal matche Program.cs)
                claims: claims,                      // Informationerne pakket ind i tokenet
                expires: DateTime.Now.AddHours(2),   // Token udløber efter 2 timer
                signingCredentials: creds            // Signaturen der beviser tokenet er ægte
            );

            // Konverterer token-objektet til en Base64-kodet streng (den lange "eyJ..." tekst)
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // Hjælpeklasse til at modtage JSON data fra login-request body

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
