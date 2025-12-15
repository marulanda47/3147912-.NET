using System;
using System.Collections.Generic;

namespace Reportes.Models;

public partial class Domicilio
{
    public int Id { get; set; }

    public int? PedidoId { get; set; }

    public int? ZonaEntregaId { get; set; }

    public int? RepartidorId { get; set; }

    public int EstadoDomicilio { get; set; }

    public DateTime HoraSolicitud { get; set; }

    public DateTime? HoraAsignacion { get; set; }

    public DateTime? HoraSalida { get; set; }

    public DateTime? HoraEntrega { get; set; }

    public decimal CostoDomicilio { get; set; }

    public decimal? Propina { get; set; }

    public string? Notas { get; set; }

    public virtual Pedido? Pedido { get; set; }

    public virtual Repartidore? Repartidor { get; set; }

    public virtual ZonasEntrega? ZonaEntrega { get; set; }
}
