using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Interfaces.Reglas;
using EventoTickets.Abstracciones.Interfaces.Servicios;
using EventoTickets.DA.Repositorios;
using EventoTickets.Flujo;
using EventoTickets.Reglas;
using EventoTickets.Servicios;

namespace EventoTickets.API.Registro;

/// <summary>MÓDULO 4 — Pagos, auditoría y entradas digitales QR (HU-15 a HU-19).</summary>
public static class Modulo4Registro
{
    public static IServiceCollection AgregarModulo4PagosQR(this IServiceCollection servicios)
    {
        servicios.AddScoped<IPagoDA, PagoDA>();
        servicios.AddScoped<IEntradaDA, EntradaDA>();
        servicios.AddSingleton<IPagoReglas, PagoReglas>();
        servicios.AddScoped<IServicioArchivosComprobantes, ServicioArchivosComprobantes>();
        servicios.AddSingleton<IServicioQr, ServicioQr>();
        servicios.AddScoped<IPagoFlujo, PagoFlujo>();
        servicios.AddScoped<IEntradaFlujo, EntradaFlujo>();
        return servicios;
    }
}
