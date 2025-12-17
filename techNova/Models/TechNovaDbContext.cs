using Microsoft.EntityFrameworkCore;

namespace techNova.Models
{
    public partial class TechNovaDbContext : DbContext
    {
        public TechNovaDbContext()
        {
        }

        public TechNovaDbContext(DbContextOptions<TechNovaDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Cliente> Clientes { get; set; }
        public virtual DbSet<DetallesVentum> DetallesVenta { get; set; }
        public virtual DbSet<Producto> Productos { get; set; }
        public virtual DbSet<Venta> Ventas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Clientes__3214EC075250ADD8");

                entity.Property(e => e.Correo).HasMaxLength(150);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.NombreCompleto).HasMaxLength(150);
                entity.Property(e => e.Telefono).HasMaxLength(50);
            });

            modelBuilder.Entity<DetallesVentum>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Detalles__3214EC07FE0270AE");

                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Producto).WithMany(p => p.DetallesVenta)
                    .HasForeignKey(d => d.ProductoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__DetallesV__Produ__534D60F1");

                entity.HasOne(d => d.Venta).WithMany(p => p.DetallesVenta)
                    .HasForeignKey(d => d.VentaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__DetallesV__Venta__52593CB8");
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC07322D7718");

                entity.HasIndex(e => e.Codigo, "UQ__Producto__06370DAC8FE595DA").IsUnique();

                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Descripcion).HasMaxLength(300);
                entity.Property(e => e.Nombre).HasMaxLength(150);
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<Venta>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Ventas__3214EC07398307C5");

                entity.Property(e => e.Fecha).HasColumnType("datetime");
                entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Cliente).WithMany(p => p.Venta)
                    .HasForeignKey(d => d.ClienteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Ventas__ClienteI__4F7CD00D");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
