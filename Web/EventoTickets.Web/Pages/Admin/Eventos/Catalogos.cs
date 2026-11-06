using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace EventoTickets.Web.Pages.Admin.Eventos;

/// <summary>Carga en ViewData las listas de categorías y locaciones para el formulario de eventos.</summary>
internal static class Catalogos
{
    public static async Task CargarAsync(ViewDataDictionary viewData, ICategoriaServicio categorias, ILocacionServicio locaciones)
    {
        var c = await categorias.ListarAsync(true);
        var l = await locaciones.ListarAsync(true);
        viewData["Categorias"] = new SelectList(c.Datos ?? new(), nameof(CategoriaResponse.CategoriaId), nameof(CategoriaResponse.Nombre));
        viewData["Locaciones"] = new SelectList((l.Datos ?? new()).Select(x => new { x.LocacionId, Texto = $"{x.Nombre} · aforo {x.CapacidadMaxima:N0}" }),
            "LocacionId", "Texto");
    }
}
