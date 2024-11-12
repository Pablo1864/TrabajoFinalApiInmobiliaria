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
    public class ContratosController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        public ContratosController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [Authorize]
        [HttpGet("inmuebles-alquilados")]
        public async Task<IActionResult> GetInmueblesAlquilados()
        {

            var propietarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(propietarioIdClaim))
            {
                return Unauthorized();
            }

            int propietarioId = int.Parse(propietarioIdClaim);

            var contratosActivos = await _context.Contratos
                .Include(c => c.Inmueble)
                .Include(c => c.Inquilino)
                .Where(c => c.Estado == true && (c.FechaFinalizacion == null || c.FechaFinalizacion > DateTime.Now)
                            && c.Inmueble.IdPropietario == propietarioId) // Filtra por propietario
                .Select(c => new
                {
                    ContratoId = c.Id,
                    InmuebleId = c.IdInmueble,
                    InquilinoId = c.InquilinoId,
                    Monto = c.Monto,
                    FechaDesde = c.FechaDesde,
                    FechaHasta = c.FechaHasta,
                    Inmueble = new
                    {
                        c.Inmueble.IdInmueble,
                        c.Inmueble.Direccion,
                        c.Inmueble.Img
                    },
                    Inquilino = new
                    {
                        c.Inquilino.Id,
                        c.Inquilino.Nombre,
                        // Agrega más propiedades si es necesario
                    }
                })
                .ToListAsync();

            return Ok(contratosActivos);
        }

        [Authorize]
        [HttpGet("contrato-detalle/{id}")]
        public async Task<IActionResult> GetContratoDetalle(int id)
        {
            var propietarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(propietarioIdClaim))
            {
                return Unauthorized();
            }

            int propietarioId = int.Parse(propietarioIdClaim);

            var contrato = await _context.Contratos
                .Include(c => c.Inmueble)
                .Include(c => c.Inquilino)
                .Where(c => c.Id == id && c.Inmueble.IdPropietario == propietarioId) // Filtra por propietario
                .FirstOrDefaultAsync();

            if (contrato == null)
            {
                return NotFound();
            }

            var contratoDetalle = new
            {
                ContratoId = contrato.Id,
                InmuebleId = contrato.IdInmueble,
                InquilinoId = contrato.InquilinoId,
                Monto = contrato.Monto,
                FechaDesde = contrato.FechaDesde,
                FechaHasta = contrato.FechaHasta,
                FechaFinalizacion = contrato.FechaFinalizacion,
                Estado = contrato.Estado,
                Inmueble = new
                {
                    contrato.Inmueble.IdInmueble,
                    contrato.Inmueble.Direccion,
                    // Agrega más detalles del inmueble si es necesario
                },
                Inquilino = new
                {
                    contrato.Inquilino.Id,
                    contrato.Inquilino.Nombre,
                    // Agrega más detalles del inquilino si es necesario
                }
            };

            return Ok(contratoDetalle);
        }
    }
}