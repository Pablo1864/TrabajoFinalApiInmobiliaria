using System.ComponentModel.DataAnnotations;

namespace ApiNet.Models;
public class CambiarContrasena
{
    public string ClaveActual { get; set; }
    public string ClaveNueva { get; set; }
}