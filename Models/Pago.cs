using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json.Serialization;

public class Pago
{
    [Key]
    [Display(Name = "Código Int.")]
    public int Id { get; set; }

    [Required]

    [Display(Name = "Fecha de pago")]
    public DateTime FechaPago { get; set; }

    public int NroPago { get; set; }

    [Required]
    [Display(Name = "Detalle del pago")]
    public string? DetallePago { get; set; }

    [Required]
    public decimal Importe { get; set; }

    [Column("ContratoId")]
    public int ContratoId { get; set; }

    [ForeignKey(nameof(ContratoId))]
    public bool Estado { get; set; }

    // Propiedad calculada para devolver la fecha sin hora en el formato deseado
    [NotMapped] // No se almacena en la base de datos
    public string FechaPagoFormateada => FechaPago.ToString("yyyy-MM-dd");
}
