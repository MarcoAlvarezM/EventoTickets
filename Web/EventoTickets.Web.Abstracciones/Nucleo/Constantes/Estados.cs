namespace EventoTickets.Web.Abstracciones.Constantes;


/// <summary>Dominios de valores definidos por las restricciones CHECK de la base de datos.</summary>
public static class EstadosUsuario
{
    public const string Activo = "Activo";
    public const string Bloqueado = "Bloqueado";
    public const string Inactivo = "Inactivo";   // borrado lógico
    public static readonly string[] Todos = { Activo, Bloqueado, Inactivo };
}

public static class EstadosEvento
{
    public const string Borrador = "Borrador";
    public const string Publicado = "Publicado";
    public const string Agotado = "Agotado";       // automático (trigger) cuando el cupo llega a cero
    public const string Cancelado = "Cancelado";
    public const string Finalizado = "Finalizado";
    public static readonly string[] Todos = { Borrador, Publicado, Agotado, Cancelado, Finalizado };
}

public static class EstadosPedido
{
    public const string PendientePago = "PendientePago";
    public const string EnValidacion = "EnValidacion";
    public const string Pagado = "Pagado";
    public const string Cancelado = "Cancelado";
    public const string Vencido = "Vencido";
    public static readonly string[] Todos = { PendientePago, EnValidacion, Pagado, Cancelado, Vencido };
}

public static class EstadosPago
{
    public const string PendienteValidacion = "PendienteValidacion";
    public const string Aprobado = "Aprobado";
    public const string Rechazado = "Rechazado";
    public static readonly string[] Todos = { PendienteValidacion, Aprobado, Rechazado };
}

public static class EstadosEntrada
{
    public const string Valida = "Valida";
    public const string Utilizada = "Utilizada";
    public const string Anulada = "Anulada";
    public static readonly string[] Todos = { Valida, Utilizada, Anulada };
}

public static class TiposDocumento
{
    public const string Cedula = "CEDULA";
    public const string Dimex = "DIMEX";
    public const string Pasaporte = "PASAPORTE";
    public static readonly string[] Todos = { Cedula, Dimex, Pasaporte };
}

public static class MetodosPago
{
    public const string SinpeMovil = "SINPE Movil";
    public const string Transferencia = "Transferencia Bancaria";
    public static readonly string[] Todos = { SinpeMovil, Transferencia };
}
