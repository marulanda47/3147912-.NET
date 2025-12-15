using System;
using System.Collections.Generic;

namespace Reportes.Models;

public partial class ZonasEntrega
{
    public int Id { get; set; }

    public string NombreZona { get; set; } = null!;

    public decimal CostoDomicilioBase { get; set; }

    public int TiempoEstimadoMinutos { get; set; }

    public virtual ICollection<Domicilio> Domicilios { get; set; } = new List<Domicilio>();
}
