using System.ComponentModel.DataAnnotations;
using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.Flujo;
using EventoTickets.Pruebas.Nucleo;
using EventoTickets.Reglas;
using Xunit;

namespace EventoTickets.Pruebas.Modulo1_Usuarios;

/// <summary>HU-01 registro · HU-02 login · HU-03 administración de usuarios.</summary>
[Trait("Modulo", "M1")]
public class UsuariosPruebas
{
    private const string HashSemilla = "PBKDF2-SHA256$100000$o/HCnntdTo+aCxwtPk9QYQ==$vsUzxa7rfGiibxm2Y7XA9cwZOvA4dBrZU3+4HlUkSJ8=";
    private readonly PasswordReglas _password = new();

    [Fact]
    public void Hash_de_la_semilla_verifica_la_clave_documentada() => Assert.True(_password.Verificar("Boleteria2026!", HashSemilla));

    [Fact]
    public void Clave_incorrecta_no_verifica() => Assert.False(_password.Verificar("otra-clave", HashSemilla));

    [Fact]
    public void Hash_nuevo_no_contiene_la_clave_y_usa_sal_aleatoria()
    {
        var a = _password.GenerarHash("Segura#2026");
        var b = _password.GenerarHash("Segura#2026");
        Assert.StartsWith("PBKDF2-SHA256$100000$", a);
        Assert.DoesNotContain("Segura#2026", a);
        Assert.NotEqual(a, b);
        Assert.True(_password.Verificar("Segura#2026", a));
    }

    [Theory]
    [InlineData("texto-plano")]
    [InlineData("PBKDF2-SHA256$abc$x$y")]
    [InlineData("")]
    public void Hash_malformado_no_verifica(string almacenado) => Assert.False(_password.Verificar("x", almacenado));

    [Fact]
    public void Cuenta_bloqueada_no_inicia_sesion()
    {
        var ex = Assert.Throws<AutenticacionException>(() => new UsuarioReglas().ValidarCuentaHabilitada(EstadosUsuario.Bloqueado));
        Assert.Contains("bloqueada", ex.Message);
    }

    [Fact]
    public void Cuenta_eliminada_logicamente_no_inicia_sesion() =>
        Assert.Throws<AutenticacionException>(() => new UsuarioReglas().ValidarCuentaHabilitada(EstadosUsuario.Inactivo));

    [Fact]
    public void Administrador_no_puede_bloquearse_a_si_mismo() =>
        Assert.Throws<ReglaNegocioException>(() => new UsuarioReglas().ValidarAutogestion(1, 1, Roles.Superadministrador, Roles.Superadministrador, EstadosUsuario.Bloqueado));

    [Fact]
    public void Administrador_no_puede_quitarse_el_rol() =>
        Assert.Throws<ReglaNegocioException>(() => new UsuarioReglas().ValidarAutogestion(1, 1, Roles.Superadministrador, Roles.Comprador, EstadosUsuario.Activo));

    [Fact]
    public void Administrador_si_puede_bloquear_a_otro_usuario() =>
        new UsuarioReglas().ValidarAutogestion(1, 8, Roles.Comprador, Roles.Comprador, EstadosUsuario.Bloqueado);

    private static (AutenticacionFlujo Flujo, Fake<IUsuarioDA> DA) CrearFlujo()
    {
        var (da, control) = Fake<IUsuarioDA>.Crear();
        return (new AutenticacionFlujo(da, new PasswordReglas(), new UsuarioReglas()), control);
    }

    [Fact]
    public async Task Login_con_correo_inexistente_y_clave_incorrecta_dan_el_mismo_mensaje()
    {
        var (flujo, da) = CrearFlujo();
        da.Cuando("ObtenerCredencialesAsync", a => (string)a[0]! == "admin@boleteriapro.cr"
            ? new CredencialesUsuario { UsuarioId = 1, PasswordHash = HashSemilla, Estado = EstadosUsuario.Activo } : null);

        var inexistente = await Assert.ThrowsAsync<AutenticacionException>(() => flujo.AutenticarAsync(new LoginRequest { CorreoElectronico = "nadie@x.cr", Password = "Boleteria2026!" }));
        var incorrecta = await Assert.ThrowsAsync<AutenticacionException>(() => flujo.AutenticarAsync(new LoginRequest { CorreoElectronico = "admin@boleteriapro.cr", Password = "mala" }));
        Assert.Equal(inexistente.Message, incorrecta.Message);
    }

    [Fact]
    public async Task Login_correcto_devuelve_el_usuario()
    {
        var (flujo, da) = CrearFlujo();
        da.Cuando("ObtenerCredencialesAsync", _ => new CredencialesUsuario { UsuarioId = 1, PasswordHash = HashSemilla, Estado = EstadosUsuario.Activo })
          .Cuando("ObtenerAsync", _ => new UsuarioResponse { UsuarioId = 1, Rol = Roles.Superadministrador });
        var usuario = await flujo.AutenticarAsync(new LoginRequest { CorreoElectronico = " admin@boleteriapro.cr ", Password = "Boleteria2026!" });
        Assert.Equal(1, usuario.UsuarioId);
        Assert.Equal("admin@boleteriapro.cr", (string)da.Argumentos("ObtenerCredencialesAsync")[0]!);
    }

    [Fact]
    public async Task Registro_guarda_hash_rol_Comprador_y_correo_normalizado()
    {
        var (flujo, da) = CrearFlujo();
        da.Cuando("CrearAsync", _ => 42).Cuando("ObtenerAsync", _ => new UsuarioResponse { UsuarioId = 42 });
        await flujo.RegistrarCompradorAsync(new RegistroCompradorRequest { CorreoElectronico = "Nuevo@Correo.COM", Password = "Segura#2026" });
        var args = da.Argumentos("CrearAsync");
        Assert.Equal("nuevo@correo.com", ((UsuarioBase)args[0]!).CorreoElectronico);
        Assert.StartsWith("PBKDF2-SHA256$", (string)args[1]!);
        Assert.Equal(Roles.Comprador, (string)args[2]!);
        Assert.Equal(EstadosUsuario.Activo, (string)args[3]!);
    }

    private static List<ValidationResult> Validar(object modelo)
    {
        var errores = new List<ValidationResult>();
        Validator.TryValidateObject(modelo, new ValidationContext(modelo), errores, true);
        return errores;
    }

    private static RegistroCompradorRequest RegistroValido() => new()
    {
        TipoDocumento = TiposDocumento.Cedula, NumeroDocumento = "1-2345-6789", NombreCompleto = "Ana Pérez",
        CorreoElectronico = "ana@correo.com", Password = "Segura#2026", ConfirmarPassword = "Segura#2026", AceptaTerminos = true
    };

    [Fact]
    public void Registro_valido_no_tiene_errores() => Assert.Empty(Validar(RegistroValido()));

    [Theory]
    [InlineData("NumeroDocumento", "123")]
    [InlineData("Password", "corta")]
    [InlineData("CorreoElectronico", "sin-arroba")]
    public void Registro_rechaza_datos_invalidos(string campo, string valor)
    {
        var modelo = RegistroValido();
        typeof(RegistroCompradorRequest).GetProperty(campo)!.SetValue(modelo, valor);
        if (campo == "Password") modelo.ConfirmarPassword = valor;
        Assert.Contains(campo, Validar(modelo).SelectMany(e => e.MemberNames));
    }

    [Fact]
    public void Registro_exige_aceptar_terminos()
    {
        var modelo = RegistroValido();
        modelo.AceptaTerminos = false;
        Assert.Contains(nameof(RegistroCompradorRequest.AceptaTerminos), Validar(modelo).SelectMany(e => e.MemberNames));
    }
}
