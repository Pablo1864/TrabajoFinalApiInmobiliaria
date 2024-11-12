using System;
using System.ComponentModel.DataAnnotations;
using PenalozaFernandezInmobiliario.Models;
using System.ComponentModel.DataAnnotations.Schema;
using ApiNet.Models;


public class Contrato
{
    [Key]
    [Display(Name = "Código Int.")]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Fecha de inicio")]
    public DateTime FechaDesde { get; set; }

    [Required]
    [Display(Name = "Fecha estipulada de finalización")]
    public DateTime FechaHasta { get; set; }
    [Display(Name = "Fecha de finalización")]
    public DateTime? FechaFinalizacion { get; set; }
    [Display(Name = "Precio mensual")]
    public decimal Monto { get; set; } //Monto mensual, no total, el total puede ser calculado en base a la cant de meses


    [Display(Name = "Inquilino")]
    public int InquilinoId { get; set; }

    [ForeignKey(nameof(InquilinoId))]
    public Inquilino? Inquilino { get; set; }



    [Display(Name = "Inmueble")]
    public int IdInmueble { get; set; }

    [ForeignKey(nameof(IdInmueble))]
    public Inmueble? Inmueble { get; set; }


    public IList<Pago>? Pagos { get; set; }

    public bool Estado { get; set; }

}