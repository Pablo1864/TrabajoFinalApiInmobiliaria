
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Linq;
using System.Threading.Tasks;
using ApiNet.Models;
using PenalozaFernandezInmobiliario.Models;

namespace PenalozaFernandezInmobiliario.Api;

[Route("api/[controller]")]
[ApiController]
public class PagosController : ControllerBase
{
    private readonly MyDbContext _context;
    private readonly IConfiguration _configuration;

    public PagosController(MyDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("ListarPagos/{idContrato}")]
    [Authorize]
    public async Task<ActionResult<List<Pago>>> ListarPagosPorContrato(int idContrato)
    {
        var propietarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(propietarioIdClaim))
        {
            return Unauthorized();
        }

        int propietarioId = int.Parse(propietarioIdClaim);
        var pagos = await _context.Pagos
            .Where(p => p.ContratoId == idContrato && p.Estado)
            .OrderBy(p => p.FechaPago)
            .ToListAsync();

        if (pagos == null || !pagos.Any())
        {
            return NotFound("No se encontraron pagos para el contrato especificado.");
        }

        return Ok(pagos);
    }

}