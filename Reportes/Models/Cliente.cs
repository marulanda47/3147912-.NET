using System;
using System.Collections.Generic;

namespace Reportes.Models;

public partial class Cliente
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
