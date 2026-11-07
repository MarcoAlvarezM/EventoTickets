using System.Reflection;

namespace EventoTickets.Pruebas.Nucleo;

/// <summary>
/// Doble de prueba escrito a mano (sin librerías de mocking) basado en <see cref="DispatchProxy"/>.
/// Se configura por nombre de método; los no configurados devuelven el valor por defecto
/// (Task completada, Task&lt;T&gt; con default, colecciones vacías).
/// </summary>
public class Fake<T> : DispatchProxy where T : class
{
    private readonly Dictionary<string, Func<object?[], object?>> _metodos = new();
    public List<(string Metodo, object?[] Argumentos)> Llamadas { get; } = new();

    public static (T Objeto, Fake<T> Control) Crear()
    {
        var objeto = Create<T, Fake<T>>();
        return (objeto, (Fake<T>)(object)objeto);
    }

    public Fake<T> Cuando(string metodo, Func<object?[], object?> respuesta)
    {
        _metodos[metodo] = respuesta;
        return this;
    }

    public int Veces(string metodo) => Llamadas.Count(l => l.Metodo == metodo);
    public object?[] Argumentos(string metodo) => Llamadas.Last(l => l.Metodo == metodo).Argumentos;

    protected override object? Invoke(MethodInfo? metodo, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(metodo);
        args ??= Array.Empty<object?>();
        lock (Llamadas) Llamadas.Add((metodo.Name, args));
        var tipo = metodo.ReturnType;
        try
        {
            var valor = _metodos.TryGetValue(metodo.Name, out var f) ? f(args) : PorDefecto(tipo);
            return Envolver(tipo, valor);
        }
        catch (Exception ex) when (typeof(Task).IsAssignableFrom(tipo))
        {
            return Fallar(tipo, ex);
        }
    }

    private static object? PorDefecto(Type tipo)
    {
        if (tipo == typeof(void) || tipo == typeof(Task)) return null;
        var interno = tipo.IsGenericType && tipo.GetGenericTypeDefinition() == typeof(Task<>) ? tipo.GetGenericArguments()[0] : tipo;
        if (interno.IsGenericType && interno.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return Array.CreateInstance(interno.GetGenericArguments()[0], 0);
        return interno.IsValueType ? Activator.CreateInstance(interno) : null;
    }

    private static object? Envolver(Type tipo, object? valor)
    {
        if (tipo == typeof(Task)) return valor as Task ?? Task.CompletedTask;
        if (tipo.IsGenericType && tipo.GetGenericTypeDefinition() == typeof(Task<>))
        {
            if (valor is Task) return valor;
            var interno = tipo.GetGenericArguments()[0];
            return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(interno).Invoke(null, new[] { valor });
        }
        return valor;
    }

    private static object Fallar(Type tipo, Exception ex)
    {
        if (tipo == typeof(Task)) return Task.FromException(ex);
        var interno = tipo.GetGenericArguments()[0];
        return typeof(Task).GetMethods().First(m => m.Name == nameof(Task.FromException) && m.IsGenericMethod)
            .MakeGenericMethod(interno).Invoke(null, new object[] { ex })!;
    }
}

/// <summary>Atributo para pruebas contra SQL Server real: se omiten si no existe la variable EVENTCR_PRUEBAS_SQL.</summary>
public sealed class FactConSqlServerAttribute : Xunit.FactAttribute
{
    public const string Variable = "EVENTCR_PRUEBAS_SQL";
    public FactConSqlServerAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Variable)))
            Skip = $"Requiere SQL Server: defina {Variable} con la cadena de conexión a EventCR_BoleteriaPro_DB.";
    }
}

public static class Opciones
{
    public static Microsoft.Extensions.Options.IOptions<TOpc> De<TOpc>(TOpc valor) where TOpc : class =>
        Microsoft.Extensions.Options.Options.Create(valor);
}
