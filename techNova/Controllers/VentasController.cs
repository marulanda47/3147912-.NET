using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using techNova.Models;

namespace techNova.Controllers
{
    public class VentasController : Controller
    {
        private readonly TechNovaDbContext _context;

        public VentasController(TechNovaDbContext context)
        {
            _context = context;
        }

        // ============================================
        // LISTADO DE VENTAS
        // ============================================
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.DetallesVenta)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(v =>
                    v.Cliente.NombreCompleto.Contains(search) ||
                    v.Fecha.ToString().Contains(search));
            }

            ViewData["Search"] = search;

            return View(await query.ToListAsync());
        }

        // ============================================
        // DETALLES DE UNA VENTA
        // ============================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (venta == null)
                return NotFound();

            return View(venta);
        }

        // ============================================
        // GET: CREAR VENTA
        // ============================================
        public IActionResult Create()
        {
            CargarCombos();
            return View();
        }

        // ============================================
        // POST: CREAR VENTA (CON VALIDACIÓN DE STOCK)
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int ClienteId,
            List<int> ProductosSeleccionados,
            List<int> Cantidades)
        {
            // Validaciones básicas
            if (ClienteId <= 0)
            {
                ModelState.AddModelError("", "Debe seleccionar un cliente.");
            }

            if (ProductosSeleccionados == null || ProductosSeleccionados.Count == 0 ||
                Cantidades == null || Cantidades.Count == 0)
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto.");
            }

            // Si ya hay error hasta aquí, recargamos combos y devolvemos vista
            if (!ModelState.IsValid)
            {
                CargarCombos(ClienteId);
                return View();
            }

            int lineas = Math.Min(ProductosSeleccionados.Count, Cantidades.Count);

            // ====================================
            // VALIDAR STOCK ANTES DE GUARDAR NADA
            // ====================================
            for (int i = 0; i < lineas; i++)
            {
                var prodId = ProductosSeleccionados[i];
                var cant = Cantidades[i];

                if (prodId <= 0)
                    continue; // fila vacía

                var producto = await _context.Productos.FindAsync(prodId);

                if (producto == null)
                {
                    ModelState.AddModelError("",
                        $"El producto con Id {prodId} no existe.");
                    continue;
                }

                if (cant <= 0)
                {
                    ModelState.AddModelError("",
                        $"La cantidad para {producto.Nombre} debe ser mayor que 0.");
                }

                if (cant > producto.Stock)
                {
                    ModelState.AddModelError("",
                        $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}, solicitado: {cant}.");
                }
            }

            // Si hay errores de stock u otros, regresamos
            if (!ModelState.IsValid)
            {
                CargarCombos(ClienteId);
                return View();
            }

            // ====================================
            // SI TODO ESTÁ BIEN: CREAR VENTA
            // ====================================
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var venta = new Venta
                {
                    ClienteId = ClienteId,
                    Fecha = DateTime.Now,
                    Total = 0
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync(); // para obtener venta.Id

                decimal total = 0m;

                for (int i = 0; i < lineas; i++)
                {
                    var prodId = ProductosSeleccionados[i];
                    var cant = Cantidades[i];

                    if (prodId <= 0 || cant <= 0)
                        continue;

                    var producto = await _context.Productos.FindAsync(prodId);

                    if (producto == null)
                        continue;

                    // Seguridad adicional por si algo cambió entre validación y aquí
                    if (cant > producto.Stock)
                    {
                        throw new InvalidOperationException(
                            $"Stock insuficiente para {producto.Nombre} al momento de guardar.");
                    }

                    decimal subtotal = producto.PrecioUnitario * cant;
                    total += subtotal;

                    var detalle = new DetallesVentum
                    {
                        ProductoId = prodId,
                        VentaId = venta.Id,
                        Cantidad = cant,
                        PrecioUnitario = producto.PrecioUnitario,
                        Subtotal = subtotal
                    };

                    _context.DetallesVenta.Add(detalle);

                    // RESTAR STOCK (ya validado)
                    producto.Stock -= cant;
                    _context.Productos.Update(producto);
                }

                venta.Total = total;
                _context.Ventas.Update(venta);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Ocurrió un error al guardar la venta. Intente de nuevo.");
                CargarCombos(ClienteId);
                return View();
            }
        }

        // ============================================
        // GET: ELIMINAR VENTA
        // ============================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (venta == null)
                return NotFound();

            return View(venta);
        }

        // ============================================
        // POST: CONFIRMAR ELIMINAR
        // ============================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.DetallesVenta)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound();

            // Restaurar stock al eliminar venta
            foreach (var detalle in venta.DetallesVenta)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId);

                if (producto != null)
                {
                    producto.Stock += detalle.Cantidad;
                    _context.Productos.Update(producto);
                }
            }

            // Eliminar detalles
            _context.DetallesVenta.RemoveRange(venta.DetallesVenta);

            // Eliminar venta
            _context.Ventas.Remove(venta);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ============================================
        // MÉTODOS PRIVADOS
        // ============================================
        private void CargarCombos(int? clienteId = null)
        {
            ViewData["ClienteId"] = new SelectList(
                _context.Clientes.OrderBy(c => c.NombreCompleto),
                "Id",
                "NombreCompleto",
                clienteId
            );

            ViewBag.Productos = _context.Productos
                .OrderBy(p => p.Nombre)
                .ToList();
        }

        private bool VentaExists(int id)
        {
            return _context.Ventas.Any(e => e.Id == id);
        }
    }
}
