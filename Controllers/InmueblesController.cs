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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InmueblesController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        public InmueblesController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("misInmuebles")]
        [Authorize]
        public IActionResult GetMisInmuebles()
        {
            var propietarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(propietarioIdClaim))
            {
                return Unauthorized();
            }

            int propietarioId = int.Parse(propietarioIdClaim);
            var inmuebles = _context.Inmuebles.Where(i => i.IdPropietario == propietarioId).ToList();

            if (inmuebles == null || !inmuebles.Any())
            {
                return NotFound(new { message = "No se encontraron inmuebles para el propietario." });
            }

            return Ok(inmuebles);
        }


        [HttpPost("agregarInmueble")]
        [Authorize]
        public async Task<IActionResult> AgregarInmueble([FromForm] Inmueble inmuebleRequest, IFormFile imagen)
        {
            var propietarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(propietarioIdClaim))
            {
                return Unauthorized();
            }

            int propietarioId = int.Parse(propietarioIdClaim);

            // Guardar la imagen en wwwroot/uploads
            string filePath = null;
            if (imagen != null)
            {
                // Crear la carpeta si no existe
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generar un nombre de archivo único y guardar la imagen
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(fileStream);
                }

                // Guardar solo la ruta relativa en la base de datos
                filePath = "/uploads/" + uniqueFileName;
            }

            var nuevoInmueble = new Inmueble
            {
                IdPropietario = propietarioId,
                IdTipoInmueble = inmuebleRequest.IdTipoInmueble,
                Uso = inmuebleRequest.Uso,
                Precio = inmuebleRequest.Precio,
                Direccion = inmuebleRequest.Direccion,
                Ambientes = inmuebleRequest.Ambientes,
                Estado = inmuebleRequest.Estado ?? "Disponible",
                Img = filePath // Ruta relativa de la imagen
            };

            _context.Inmuebles.Add(nuevoInmueble);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetMisInmuebles), new { id = nuevoInmueble.IdInmueble }, nuevoInmueble);
        }


        [HttpPut("actualizarEstado/{idInmueble}")]
        [Authorize]
        public IActionResult ActualizarEstado(int idInmueble, [FromBody] string nuevoEstado)
        {
            // Obtener el ID del propietario logueado desde los claims
            var propietarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(propietarioIdClaim))
            {
                return Unauthorized();
            }

            int propietarioId = int.Parse(propietarioIdClaim);

            // Buscar el inmueble por ID
            var inmueble = _context.Inmuebles.FirstOrDefault(i => i.IdInmueble == idInmueble);
            if (inmueble == null)
            {
                return NotFound(new { message = "Inmueble no encontrado" });
            }

            // Verificar que el inmueble pertenece al propietario logueado
            if (inmueble.IdPropietario != propietarioId)
            {
                return Forbid("No tienes permiso para modificar este inmueble.");
            }

            // Actualizar el estado del inmueble
            inmueble.Estado = nuevoEstado;
            _context.SaveChanges();

            return Ok(new { message = "Estado actualizado correctamente", inmueble });
        }
    }
}

