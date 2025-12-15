
using System.Collections.Generic;

namespace Reportes.Models.ViewModels
{
    public class TopClientesViewModel
    {
        public int Anio { get; set; }
        public int Mes { get; set; }

        public List<TopClienteVm> Clientes { get; set; } = new();

        public string LabelsJson { get; set; } = "[]";
        public string DatosJson { get; set; } = "[]";
    }

    public class TopClienteVm
    {
        public string Nombre { get; set; } = "";
        public int CantidadPedidos { get; set; }
        public decimal TotalGastado { get; set; }
    }
}
