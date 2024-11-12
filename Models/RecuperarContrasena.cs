using System.ComponentModel.DataAnnotations;

namespace ApiNet.Models;

public class RecuperarContrasena
{

    public string? Email { get; set; }

    public string? Token { get; set; }

    //[MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string? NuevaContrasena { get; set; }

    [Compare("NuevaContrasena", ErrorMessage = "Las contraseñas no coinciden.")]
    public string? ConfirmarContrasena { get; set; }
}

