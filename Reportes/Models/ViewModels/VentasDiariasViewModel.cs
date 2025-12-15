using System;
using System.Collections.Generic;

namespace Reportes.Models.ViewModels
{
    public class VentasDiariasViewModel
    {
        public DateTime Fecha { get; set; }

        public decimal TotalDia { get; set; }
        public int CantidadPedidos { get; set; }
        public decimal TicketPromedio { get; set; }

        public List<PedidoResumenVm> Pedidos { get; set; } = new();

        // Gráfico por método de pago
        public string LabelsMetodosPagoJson { get; set; } = "[]";
        public string DatosMetodosPagoJson { get; set; } = "[]";
    }

    public class PedidoResumenVm
    {
        public int Id { get; set; }
        public string ClienteNombre { get; set; } = "";
        public DateTime FechaPedido { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; } = "";
    }
}
