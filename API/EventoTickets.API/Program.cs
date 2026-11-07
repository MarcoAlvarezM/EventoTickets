using System.Reflection;
using System.Security.Claims;
using System.Text;
using EventoTickets.Abstracciones.Configuracion;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.API.Middleware;
using EventoTickets.API.Nucleo.Filtros;
using EventoTickets.API.Registro;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuración necesaria antes de construir ----------
var jwt = builder.Configuration.GetSection(ConfiguracionJwt.Seccion).Get<ConfiguracionJwt>() ?? new ConfiguracionJwt();
if (Encoding.UTF8.GetByteCount(jwt.Clave) < 32)
    throw new InvalidOperationException("Configure Jwt:Clave (mínimo 32 caracteres) en appsettings o en la variable de entorno Jwt__Clave.");

var almacenamiento = builder.Configuration.GetSection(ConfiguracionAlmacenamiento.Seccion).Get<ConfiguracionAlmacenamiento>()
                     ?? new ConfiguracionAlmacenamiento();
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = (almacenamiento.TamanoMaximoMB + 1) * 1024L * 1024L);

// ---------- Núcleo + módulos (cada integrante registra SOLO en su archivo de Registro/) ----------
builder.Services
    .AgregarNucleo(builder.Configuration)
    .AgregarModulo1Usuarios()
    .AgregarModulo2Eventos()
    .AgregarModulo3Pedidos()
    .AgregarModulo4PagosQR();

// ---------- Autenticación JWT y autorización por roles ----------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emisor,
            ValidateAudience = true,
            ValidAudience = jwt.Audiencia,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services
    .AddControllers(o => o.Filters.Add<RespuestaApiFiltro>())
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = contexto =>
    {
        // Errores de Data Annotations con el formato común RespuestaApi
        var errores = contexto.ModelState.Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());
        return new BadRequestObjectResult(RespuestaApi.Error(StatusCodes.Status400BadRequest, "Revise los datos ingresados.", null, errores));
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EventoTickets API (BoleteríaPro)",
        Version = "v1",
        Description = "API REST de gestión de eventos y venta de entradas — SC-701 Programación Avanzada en Web."
    });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue el token obtenido en POST /api/autenticacion/login"
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml)) o.IncludeXmlComments(xml);
});

var app = builder.Build();

app.UseMiddleware<ManejoExcepcionesMiddleware>();
// 401 / 403 / 404 sin cuerpo (p. ej. token ausente o ruta inexistente) también con el formato común
app.UseStatusCodePages(async contexto =>
{
    var http = contexto.HttpContext;
    var mensaje = http.Response.StatusCode switch
    {
        401 => "Debe iniciar sesión (token ausente, vencido o inválido).",
        403 => "Su rol no tiene permiso para esta operación.",
        404 => "La ruta solicitada no existe.",
        _ => "No fue posible procesar la solicitud."
    };
    await ManejoExcepcionesMiddleware.EscribirAsync(http, RespuestaApi.Error(http.Response.StatusCode, mensaje));
});
app.UseSwagger();
app.UseSwaggerUI(o => o.DocumentTitle = "EventoTickets API");
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
