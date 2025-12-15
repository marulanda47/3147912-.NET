using System;
using System.Collections.Generic;

namespace Reportes.Models.ViewModels
{
    public class VentasMensualesViewModel
    {
        public int Anio { get; set; }
        public int Mes { get; set; }

        public decimal TotalMes { get; set; }
        public int CantidadPedidos { get; set; }
        public decimal TicketPromedio { get; set; }

        // Gráfico: ventas por día del mes
        public string LabelsDiasJson { get; set; } = "[]";
        public string DatosDiasJson { get; set; } = "[]";
    }
}
