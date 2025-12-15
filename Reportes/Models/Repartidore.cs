using System;
using System.Collections.Generic;

namespace Reportes.Models;

public partial class Repartidore
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Documento { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Domicilio> Domicilios { get; set; } = new List<Domicilio>();
}
