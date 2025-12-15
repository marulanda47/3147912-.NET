using System;
using System.Collections.Generic;

namespace Reportes.Models;

public partial class Pedido
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public DateTime FechaPedido { get; set; }

    public decimal Total { get; set; }

    public int EstadoPedido { get; set; }

    public string? MetodoPago { get; set; }

    public string? Observaciones { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<DetallesPedido> DetallesPedidos { get; set; } = new List<DetallesPedido>();

    public virtual ICollection<Domicilio> Domicilios { get; set; } = new List<Domicilio>();
}
