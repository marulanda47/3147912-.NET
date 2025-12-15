using System.Collections.Generic;

namespace Reportes.Models.ViewModels
{
    public class TopProductosViewModel
    {
        public int Anio { get; set; }
        public int Mes { get; set; }

        public List<TopProductoVm> Productos { get; set; } = new();

        public string LabelsJson { get; set; } = "[]";
        public string DatosJson { get; set; } = "[]";
    }

    public class TopProductoVm
    {
        public string Nombre { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
    }
}
