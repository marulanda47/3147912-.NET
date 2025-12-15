using System;

namespace Reportes.Models.ViewModels
{
    public class DashboardViewModel
    {
        // KPIs generales
        public decimal VentasHoy { get; set; }
        public decimal VentasMes { get; set; }
        public decimal VentasTotales { get; set; }
        public int PedidosHoy { get; set; }
        public int PedidosMes { get; set; }
        public int PedidosTotales { get; set; }

        // Domicilios
        public int DomiciliosHoy { get; set; }
        public int DomiciliosEnProceso { get; set; }
        public int DomiciliosCompletadosMes { get; set; }

        // Top info
        public string? ProductoMasVendido { get; set; }
        public int ProductoMasVendidoCantidad { get; set; }

        public string? ClienteTopNombre { get; set; }
        public decimal ClienteTopTotal { get; set; }

        // Datos para gráfico (ventas últimos 7 días)
        public string LabelsUltimos7DiasJson { get; set; } = "[]";
        public string DatosUltimos7DiasJson { get; set; } = "[]";
    }
}
