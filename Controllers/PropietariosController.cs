using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiNet.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Authorization;

namespace PenalozaFernandezInmobiliario.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropietariosController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public PropietariosController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }





        [HttpPost("login")]
        public IActionResult Login([FromBody] Login request)
        {
            var propietario = _context.Propietarios.SingleOrDefault(p => p.Email == request.Email);
            if (propietario == null) return Unauthorized(new { message = "Email no registrado." });
            if (string.IsNullOrEmpty(propietario.Clave)) return Unauthorized(new { message = "La contraseña no es válida." });

            string hashedPassword = HashPassword(request.Password);
            if (hashedPassword != propietario.Clave) return Unauthorized(new { message = "La contraseña es incorrecta." });

            // Definir el tiempo de expiración, por ejemplo, 1 hora
            var expiration = TimeSpan.FromHours(1);

            // Generar el token con el propietario y el tiempo de expiración
            var token = GenerateJwtToken(propietario, expiration);

            var response = new
            {
                token,
                propietario = new
                {
                    propietario.IdPropietario,
                    propietario.Nombre,
                    propietario.Apellido,
                    propietario.Email,
                    propietario.Telefono,
                    propietario.Domicilio,
                    propietario.Dni
                }
            };
            return Ok(response);
        }

        private string GenerateJwtToken(Propietario propietario, TimeSpan expiration)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, propietario.IdPropietario.ToString()),
        new Claim(ClaimTypes.Email, propietario.Email ?? string.Empty),
        new Claim(ClaimTypes.Name, $"{propietario.Nombre} {propietario.Apellido}"),
        new Claim("TokenPurpose", "PasswordReset")  // Propósito del token
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.Add(expiration),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            string globalSalt = _configuration["Salt"];
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(globalSalt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8));
        }

        private ClaimsPrincipal ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                // Configuración de validación tomada del Program.cs
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true // Validar la expiración del token
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Validar el propósito específico del token
                var tokenPurpose = principal.Claims.FirstOrDefault(c => c.Type == "TokenPurpose")?.Value;
                if (tokenPurpose != "PasswordReset")
                {
                    return null; // El token no es válido para restablecimiento de contraseña
                }

                return principal; // Token válido y con propósito correcto
            }
            catch
            {
                return null; // Token inválido o expirado
            }
        }

        [HttpPut("editarPerfil")]
        [Authorize]
        public IActionResult UpdateLoggedUser([FromBody] PropietarioUpdateModel updatedPropietario)
        {
            var propietarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (propietarioIdClaim == null) return Unauthorized();

            int propietarioId = int.Parse(propietarioIdClaim);
            var propietario = _context.Propietarios.Find(propietarioId);
            if (propietario == null) return NotFound("Propietario no encontrado.");

            propietario.Nombre = updatedPropietario.Nombre ?? propietario.Nombre;
            propietario.Apellido = updatedPropietario.Apellido ?? propietario.Apellido;
            propietario.Email = updatedPropietario.Email ?? propietario.Email;
            propietario.Telefono = updatedPropietario.Telefono ?? propietario.Telefono;
            propietario.Domicilio = updatedPropietario.Domicilio ?? propietario.Domicilio;

            _context.Propietarios.Update(propietario);
            _context.SaveChanges();

            return Ok(new { message = "Propietario actualizado con éxito.", propietarioActualizado = propietario });
        }

        [HttpPut("cambiarClave")]
        [Authorize]
        public IActionResult CambiarContraseña([FromBody] CambiarContrasena request)
        {
            var propietarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (propietarioIdClaim == null) return Unauthorized();

            int propietarioId = int.Parse(propietarioIdClaim);
            var propietario = _context.Propietarios.Find(propietarioId);
            if (propietario == null) return NotFound(new { message = "Propietario no encontrado." });

            string hashedOldPassword = HashPassword(request.ClaveActual);
            if (hashedOldPassword != propietario.Clave) return Unauthorized(new { message = "La contraseña actual es incorrecta." });

            propietario.Clave = HashPassword(request.ClaveNueva);
            _context.Propietarios.Update(propietario);
            _context.SaveChanges();

            return Ok(new { message = "Contraseña cambiada con éxito." });
        }

        [HttpPost("solicitarRecuperacion")]
        public async Task<IActionResult> SolicitarRecuperacion([FromForm] RecuperarContrasena request, [FromServices] EmailService emailService)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(new { message = "El email es requerido." });

            var propietario = _context.Propietarios.SingleOrDefault(p => p.Email == request.Email);
            if (propietario == null) return NotFound(new { message = "Email no registrado." });

            var resetToken = GenerateJwtToken(propietario, TimeSpan.FromMinutes(30));
            string subject = "Recuperación de contraseña";
            string resetLink = $"http://192.168.1.4:5000/api/Propietarios/redirigirResetPassword?token={resetToken}";
            string body = $"<html><body>Haz clic en el siguiente enlace para restablecer tu contraseña: " +
                          $"<a href='{resetLink}'>Restablecer Contraseña</a></body></html>";

            await emailService.SendEmailAsync(request.Email, subject, body);

            // Enviar el token en la respuesta para pruebas
            return Ok(new { message = "Correo enviado con instrucciones para restablecer la contraseña.", token = resetToken });
        }


        [HttpGet("redirigirResetPassword")]
        public IActionResult RedirigirResetPassword(string token)
        {
            // Redirecciona al esquema personalizado de tu app con el token como parámetro
            string appLink = $"myapp://reset-password?token={token}";
            return Redirect(appLink);
        }

        [HttpPost("restablecerContrasena")]
        public IActionResult RestablecerContrasena([FromBody] RecuperarContrasena request)
        {
            if (string.IsNullOrEmpty(request.Token))
                return BadRequest(new { message = "El token es requerido." });

            var principal = ValidateJwtToken(request.Token);
            if (principal == null) return BadRequest(new { message = "Token inválido o expirado." });

            var propietario = _context.Propietarios.Find(int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier).Value));
            if (propietario == null) return NotFound(new { message = "Propietario no encontrado." });

            if (request.NuevaContrasena != request.ConfirmarContrasena)
                return BadRequest(new { message = "Las contraseñas no coinciden." });

            propietario.Clave = HashPassword(request.NuevaContrasena);
            _context.SaveChanges();

            return Ok(new { message = "Contraseña restablecida con éxito." });
        }
    }
}

