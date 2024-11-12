using System.Configuration;
using ApiNet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000", "http://*:5000");

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuración solo para autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => // Configuración para la autenticación JWT
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],  // Cambiado
            ValidAudience = builder.Configuration["Jwt:Audience"], // Cambiado
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // Cambiado
        };
    });

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseMySql(builder.Configuration["ConnectionString"], new MariaDbServerVersion(new Version(10, 4, 32)))
);
builder.Services.AddTransient<EmailService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Inmueble/Upsert");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(x => x // Habilitar CORS (ya que vamos a llamar desde un dominio distinto(API))
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
app.UseStaticFiles();

app.UseRouting();

// Habilitar autenticación y autorización
app.UseAuthentication();  // Esta línea habilita la autenticación
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
