using System.ComponentModel.DataAnnotations;

namespace ApiNet.Models
{
    public class PropietarioUpdateModel
    {
        [EmailAddress(ErrorMessage = "Por favor, ingrese un email válido")]
        public string? Email { get; set; }

        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        public string? Domicilio { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
    }
}
