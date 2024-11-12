using System.ComponentModel.DataAnnotations;
using PenalozaFernandezInmobiliario.Models;
namespace ApiNet.Models;


public class Inmueble
{
    [Key]
    public int IdInmueble { get; set; }
    public int IdPropietario { get; set; }
    public int IdTipoInmueble { get; set; }
    public string Uso { get; set; }
    public decimal Precio { get; set; }
    public string Direccion { get; set; }
    public int Ambientes { get; set; }
    public int? Superficie { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public string? Estado { get; set; }
    public string? Img { get; set; }
}
