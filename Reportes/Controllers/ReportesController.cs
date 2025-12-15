using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reportes.Models;
using Reportes.Models.ViewModels;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Reportes.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ChocoAdminDbContext _context;

        public ReportesController(ChocoAdminDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // DASHBOARD GENERAL
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var inicioHoy = hoy.Date;
            var finHoy = inicioHoy.AddDays(1);

            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var finMes = inicioMes.AddMonths(1);

            // Ventas hoy
            var ventasHoy = await _context.Pedidos
                .Where(p => p.FechaPedido >= inicioHoy && p.FechaPedido < finHoy)
                .SumAsync(p => (decimal?)p.Total) ?? 0m;

            // Ventas mes
            var ventasMes = await _context.Pedidos
                .Where(p => p.FechaPedido >= inicioMes && p.FechaPedido < finMes)
                .SumAsync(p => (decimal?)p.Total) ?? 0m;

            // Pedidos hoy
            var pedidosHoy = await _context.Pedidos
                .CountAsync(p => p.FechaPedido >= inicioHoy && p.FechaPedido < finHoy);

            // Domicilios hoy
            var domiciliosHoy = await _context.Domicilios
                .CountAsync(d => d.HoraSolicitud >= inicioHoy && d.HoraSolicitud < finHoy);

            // Totales históricos
            var ventasTotales = await _context.Pedidos.SumAsync(p => (decimal?)p.Total) ?? 0m;
            var pedidosTotales = await _context.Pedidos.CountAsync();
            var pedidosMes = await _context.Pedidos
                .CountAsync(p => p.FechaPedido >= inicioMes && p.FechaPedido < finMes);

            // Domicilios en proceso (tomamos los que no tienen HoraEntrega)
            var domiciliosEnProceso = await _context.Domicilios
                .CountAsync(d => d.HoraEntrega == null);

            // Domicilios completados en el mes
            var domiciliosCompletadosMes = await _context.Domicilios
                .CountAsync(d => d.HoraEntrega != null &&
                                 d.HoraEntrega >= inicioMes &&
                                 d.HoraEntrega < finMes);

            // Producto más vendido (por cantidad)
            var topProd = await (from d in _context.DetallesPedidos
                                 join p in _context.Pedidos on d.PedidoId equals p.Id
                                 join prod in _context.Productos on d.ProductoId equals prod.Id
                                 where p.FechaPedido >= inicioMes && p.FechaPedido < finMes
                                 group new { d, prod } by new { d.ProductoId, prod.Nombre } into g
                                 orderby g.Sum(x => x.d.Cantidad) descending
                                 select new
                                 {
                                     Nombre = g.Key.Nombre,
                                     Cantidad = g.Sum(x => x.d.Cantidad)
                                 })
                                 .FirstOrDefaultAsync();

            string productoMasVendido = topProd?.Nombre ?? "Sin datos";
            int productoMasVendidoCantidad = topProd?.Cantidad ?? 0;

            // Cliente top del mes
            var topCli = await (from p in _context.Pedidos
                                join c in _context.Clientes on p.ClienteId equals c.Id
                                where p.FechaPedido >= inicioMes && p.FechaPedido < finMes
                                group new { p, c } by new { p.ClienteId, c.NombreCompleto } into g
                                orderby g.Sum(x => x.p.Total) descending
                                select new
                                {
                                    Nombre = g.Key.NombreCompleto,
                                    Total = g.Sum(x => x.p.Total)
                                })
                                .FirstOrDefaultAsync();

            string clienteTopNombre = topCli?.Nombre ?? "Sin datos";
            decimal clienteTopTotal = topCli?.Total ?? 0m;

            // Ventas últimos 7 días (incluyendo hoy)
            var desde7 = hoy.AddDays(-6).Date;
            var hasta7 = hoy.Date;

            var ventas7Query = await _context.Pedidos
                .Where(p => p.FechaPedido.Date >= desde7 && p.FechaPedido.Date <= hasta7)
                .GroupBy(p => p.FechaPedido.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .ToListAsync();

            var labels7 = Enumerable.Range(0, 7)
                .Select(i => desde7.AddDays(i))
                .Select(f => f.ToString("dd/MM"))
                .ToList();

            var datos7 = Enumerable.Range(0, 7)
                .Select(i => desde7.AddDays(i))
                .Select(fecha =>
                {
                    var item = ventas7Query.FirstOrDefault(x => x.Fecha == fecha);
                    return item?.Total ?? 0m;
                })
                .ToList();

            var vm = new DashboardViewModel
            {
                VentasHoy = ventasHoy,
                VentasMes = ventasMes,
                PedidosHoy = pedidosHoy,
                DomiciliosHoy = domiciliosHoy,

                VentasTotales = ventasTotales,
                PedidosTotales = pedidosTotales,
                PedidosMes = pedidosMes,

                ProductoMasVendido = productoMasVendido,
                ProductoMasVendidoCantidad = productoMasVendidoCantidad,

                ClienteTopNombre = clienteTopNombre,
                ClienteTopTotal = clienteTopTotal,

                DomiciliosEnProceso = domiciliosEnProceso,
                DomiciliosCompletadosMes = domiciliosCompletadosMes,

                LabelsUltimos7DiasJson = JsonSerializer.Serialize(labels7),
                DatosUltimos7DiasJson = JsonSerializer.Serialize(datos7)
            };

            return View(vm);
        }

        // ==========================================
        // VENTAS DIARIAS
        // ==========================================
        public async Task<IActionResult> VentasDiarias(DateTime? fecha)
        {
            var f = (fecha ?? DateTime.Today).Date;
            var inicio = f;
            var fin = f.AddDays(1);

            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.FechaPedido >= inicio && p.FechaPedido < fin)
                .OrderBy(p => p.FechaPedido)
                .ToListAsync();

            var vm = new VentasDiariasViewModel
            {
                Fecha = f,
                TotalDia = pedidos.Sum(p => p.Total),
                CantidadPedidos = pedidos.Count,
                TicketPromedio = pedidos.Count > 0 ? pedidos.Average(p => p.Total) : 0,
                Pedidos = pedidos.Select(p => new PedidoResumenVm
                {
                    Id = p.Id,
                    ClienteNombre = p.Cliente != null ? p.Cliente.NombreCompleto : "(sin cliente)",
                    FechaPedido = p.FechaPedido,
                    Total = p.Total,
                    MetodoPago = string.IsNullOrEmpty(p.MetodoPago) ? "Sin método" : p.MetodoPago
                }).ToList()
            };

            var metodos = pedidos
                .GroupBy(p => string.IsNullOrEmpty(p.MetodoPago) ? "Sin método" : p.MetodoPago)
                .Select(g => new
                {
                    Metodo = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            vm.LabelsMetodosPagoJson = JsonSerializer.Serialize(metodos.Select(x => x.Metodo));
            vm.DatosMetodosPagoJson = JsonSerializer.Serialize(metodos.Select(x => x.Total));

            return View(vm);
        }

        // ==========================================
        // VENTAS MENSUALES
        // ==========================================
        public async Task<IActionResult> VentasMensuales(int? anio, int? mes)
        {
            var hoy = DateTime.Today;
            int y = anio ?? hoy.Year;
            int m = mes ?? hoy.Month;

            var inicio = new DateTime(y, m, 1);
            var fin = inicio.AddMonths(1);

            var pedidos = await _context.Pedidos
                .Where(p => p.FechaPedido >= inicio && p.FechaPedido < fin)
                .ToListAsync();

            var vm = new VentasMensualesViewModel
            {
                Anio = y,
                Mes = m,
                TotalMes = pedidos.Sum(p => p.Total),
                CantidadPedidos = pedidos.Count,
                TicketPromedio = pedidos.Count > 0 ? pedidos.Average(p => p.Total) : 0
            };

            var porDia = pedidos
                .GroupBy(p => p.FechaPedido.Date)
                .Select(g => new
                {
                    Dia = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .OrderBy(x => x.Dia)
                .ToList();

            vm.LabelsDiasJson = JsonSerializer.Serialize(porDia.Select(x => x.Dia.ToString("dd/MM")));
            vm.DatosDiasJson = JsonSerializer.Serialize(porDia.Select(x => x.Total));

            return View(vm);
        }

        // ==========================================
        // TOP PRODUCTOS
        // ==========================================
        public async Task<IActionResult> TopProductos(int? anio, int? mes)
        {
            var hoy = DateTime.Today;
            int y = anio ?? hoy.Year;
            int m = mes ?? hoy.Month;

            var inicio = new DateTime(y, m, 1);
            var fin = inicio.AddMonths(1);

            var query = from d in _context.DetallesPedidos
                        join p in _context.Pedidos on d.PedidoId equals p.Id
                        join prod in _context.Productos on d.ProductoId equals prod.Id
                        where p.FechaPedido >= inicio && p.FechaPedido < fin
                        group new { d, prod } by new { d.ProductoId, prod.Nombre } into g
                        orderby g.Sum(x => x.d.Cantidad) descending
                        select new TopProductoVm
                        {
                            Nombre = g.Key.Nombre,
                            Cantidad = g.Sum(x => x.d.Cantidad),
                            Monto = g.Sum(x => x.d.Subtotal)
                        };

            var lista = await query.Take(10).ToListAsync();

            var vm = new TopProductosViewModel
            {
                Anio = y,
                Mes = m,
                Productos = lista,
                LabelsJson = JsonSerializer.Serialize(lista.Select(x => x.Nombre)),
                DatosJson = JsonSerializer.Serialize(lista.Select(x => x.Cantidad))
            };

            return View(vm);
        }

        // ==========================================
        // TOP CLIENTES
        // ==========================================
        public async Task<IActionResult> TopClientes(int? anio, int? mes)
        {
            var hoy = DateTime.Today;
            int y = anio ?? hoy.Year;
            int m = mes ?? hoy.Month;

            var inicio = new DateTime(y, m, 1);
            var fin = inicio.AddMonths(1);

            var query = from p in _context.Pedidos
                        join c in _context.Clientes on p.ClienteId equals c.Id
                        where p.FechaPedido >= inicio && p.FechaPedido < fin
                        group new { p, c } by new { p.ClienteId, c.NombreCompleto } into g
                        orderby g.Sum(x => x.p.Total) descending
                        select new TopClienteVm
                        {
                            Nombre = g.Key.NombreCompleto,
                            CantidadPedidos = g.Count(),
                            TotalGastado = g.Sum(x => x.p.Total)
                        };

            var lista = await query.Take(10).ToListAsync();

            var vm = new TopClientesViewModel
            {
                Anio = y,
                Mes = m,
                Clientes = lista,
                LabelsJson = JsonSerializer.Serialize(lista.Select(x => x.Nombre)),
                DatosJson = JsonSerializer.Serialize(lista.Select(x => x.TotalGastado))
            };

            return View(vm);
        }
    }
}
