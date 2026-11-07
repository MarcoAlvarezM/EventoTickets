using EventoTickets.Web.Abstracciones.Configuracion;
using EventoTickets.Web.Abstracciones.Constantes;
using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Infraestructura;
using EventoTickets.Web.Registro;
using EventoTickets.Web.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuración ----------
builder.Services.Configure<ConfiguracionApi>(builder.Configuration.GetSection(ConfiguracionApi.Seccion));
builder.Services.Configure<ConfiguracionCuentaRecaudadora>(builder.Configuration.GetSection(ConfiguracionCuentaRecaudadora.Seccion));

// ---------- Cliente HTTP hacia el API (IHttpClientFactory + cliente tipado) ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IClienteApi, ClienteApi>((proveedor, http) =>
{
    var api = proveedor.GetRequiredService<IOptions<ConfiguracionApi>>().Value;
    http.BaseAddress = new Uri(api.UrlBase.EndsWith('/') ? api.UrlBase : api.UrlBase + "/");
    http.Timeout = TimeSpan.FromSeconds(api.TimeoutSegundos);
});
// Cada integrante registra sus servicios SOLO en su archivo de Registro/
builder.Services
    .AgregarModulo1Usuarios()
    .AgregarModulo2Eventos()
    .AgregarModulo3Pedidos()
    .AgregarModulo4PagosQR();

// ---------- Sesión por cookie (guarda el JWT emitido por el API) ----------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Cuenta/Login";
        o.LogoutPath = "/Cuenta/Logout";
        o.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        o.Cookie.Name = "BoleteriaPro.Sesion";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.ExpireTimeSpan = TimeSpan.FromMinutes(120);
        o.SlidingExpiration = false;
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy(Politicas.Personal, p => p.RequireRole(Roles.Superadministrador, Roles.AdministradorEventos, Roles.AuditorPagos, Roles.ValidadorAcceso));
    o.AddPolicy(Politicas.Superadministrador, p => p.RequireRole(Roles.Superadministrador));
    o.AddPolicy(Politicas.GestionEventos, p => p.RequireRole(Roles.Superadministrador, Roles.AdministradorEventos));
    o.AddPolicy(Politicas.Supervision, p => p.RequireRole(Roles.Superadministrador, Roles.AdministradorEventos, Roles.AuditorPagos));
    o.AddPolicy(Politicas.ControlAcceso, p => p.RequireRole(Roles.Superadministrador, Roles.ValidadorAcceso));
});

// Emite tildes y ñ sin convertirlas en entidades HTML
builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

builder.Services.AddRazorPages(o =>
{
    // Área del comprador
    o.Conventions.AuthorizeFolder("/Compras");
    o.Conventions.AuthorizeFolder("/MisPedidos");
    o.Conventions.AuthorizeFolder("/MisPagos");
    o.Conventions.AuthorizeFolder("/MisEntradas");
    // Área administrativa (políticas por módulo)
    o.Conventions.AuthorizeFolder("/Admin", Politicas.Personal);
    o.Conventions.AuthorizeFolder("/Admin/Usuarios", Politicas.Superadministrador);
    o.Conventions.AuthorizeFolder("/Admin/Eventos", Politicas.GestionEventos);
    o.Conventions.AuthorizeFolder("/Admin/Categorias", Politicas.GestionEventos);
    o.Conventions.AuthorizeFolder("/Admin/Locaciones", Politicas.GestionEventos);
    o.Conventions.AuthorizeFolder("/Admin/TiposEntrada", Politicas.GestionEventos);
    o.Conventions.AuthorizeFolder("/Admin/Pedidos", Politicas.Supervision);
    o.Conventions.AuthorizeFolder("/Admin/Pagos", Politicas.Supervision);
    o.Conventions.AuthorizeFolder("/Admin/Reportes", Politicas.Supervision);
    o.Conventions.AuthorizeFolder("/Admin/Accesos", Politicas.ControlAcceso);
});

var app = builder.Build();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(Formato.Cultura),
    SupportedCultures = new[] { Formato.Cultura },
    SupportedUICultures = new[] { Formato.Cultura }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/Error", "?codigo={0}");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
