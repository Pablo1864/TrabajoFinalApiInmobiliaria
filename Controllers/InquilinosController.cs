using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Linq;
using System.Threading.Tasks;
using ApiNet.Models;
using PenalozaFernandezInmobiliario.Models;

namespace PenalozaFernandezInmobiliario.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InquilinosController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        public InquilinosController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Método para obtener inmuebles alquilados del propietario
        [HttpGet("alquilados")]
        public async Task<IActionResult> GetInmueblesAlquiladosPorPropietario()
        {
            // Obtener el idPropietario desde el token
            var propietarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(propietarioIdClaim))
            {
                return Unauthorized("No se ha encontrado el propietario en el token.");
            }

            int propietarioId;
            if (!int.TryParse(propietarioIdClaim, out propietarioId))
            {
                return BadRequest("El id del propietario no es válido.");
            }

            // Verificar si el contexto está inicializado
            if (_context == null)
            {
                return StatusCode(500, "Error al acceder a la base de datos.");
            }

            var inmueblesAlquilados = await _context.Contratos
                .Where(c => c.Estado == true // Contratos activos
                            && c.FechaHasta >= DateTime.Now // Contratos vigentes
                            && (c.FechaFinalizacion == null || c.FechaFinalizacion >= DateTime.Now)
                            && _context.Inmuebles.Any(i => i.IdInmueble == c.IdInmueble && i.IdPropietario == propietarioId))
                .Select(c => new
                {
                    c.IdInmueble,
                    Direccion = _context.Inmuebles.Where(i => i.IdInmueble == c.IdInmueble).Select(i => i.Direccion).FirstOrDefault(),
                    Precio = _context.Inmuebles.Where(i => i.IdInmueble == c.IdInmueble).Select(i => i.Precio).FirstOrDefault(),
                    c.FechaDesde,
                    c.FechaHasta
                })
                .ToListAsync();

            if (!inmueblesAlquilados.Any())
            {
                return NotFound("No hay inmuebles alquilados actualmente.");
            }

            return Ok(inmueblesAlquilados);
        }


        // Método para obtener datos del inquilino de un inmueble específico
        [HttpGet("{idInmueble}")]
        public async Task<IActionResult> GetInquilinoPorInmueble(int idInmueble)
        {
            var propietarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(propietarioIdClaim))
            {
                return Unauthorized("No se ha encontrado el propietario en el token.");
            }

            if (!int.TryParse(propietarioIdClaim, out int propietarioId))
            {
                return BadRequest("El id del propietario no es válido.");
            }

            var contrato = await _context.Contratos
              .Where(c => c.IdInmueble == idInmueble
                          && c.Estado == true
                          && c.FechaHasta >= DateTime.Now
                          && (c.FechaFinalizacion == null || c.FechaFinalizacion >= DateTime.Now)
                          && _context.Inmuebles.Any(i => i.IdInmueble == c.IdInmueble && i.IdPropietario == propietarioId))
              .Select(c => new Inquilino
              {
                  Id = c.InquilinoId,
                  Nombre = c.Inquilino.Nombre,
                  Apellido = c.Inquilino.Apellido,
                  Dni = c.Inquilino.Dni,
                  Domicilio = c.Inquilino.Domicilio,
                  Telefono = c.Inquilino.Telefono,
                  Email = c.Inquilino.Email
              })
              .FirstOrDefaultAsync();

            if (contrato == null)
            {
                return NotFound("No se encontró el inquilino para este inmueble.");
            }

            return Ok(contrato);
        }
    }
}


