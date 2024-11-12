using Microsoft.EntityFrameworkCore;

using ApiNet.Models;
using PenalozaFernandezInmobiliario.Models;


public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {

    }


    public DbSet<Propietario> Propietarios { get; set; }
    public DbSet<Inmueble> Inmuebles { get; set; }
    public DbSet<Inquilino> Inquilinos { get; set; }

    public DbSet<Contrato> Contratos { get; set; }
    public DbSet<Pago> Pagos { get; set; }



}