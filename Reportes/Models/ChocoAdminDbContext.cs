using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Reportes.Models;

public partial class ChocoAdminDbContext : DbContext
{
    public ChocoAdminDbContext()
    {
    }

    public ChocoAdminDbContext(DbContextOptions<ChocoAdminDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<DetallesPedido> DetallesPedidos { get; set; }

    public virtual DbSet<Domicilio> Domicilios { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Repartidore> Repartidores { get; set; }

    public virtual DbSet<ZonasEntrega> ZonasEntregas { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=GRYSTO\\SQLEXPRESS;Database=ChocoAdminDb;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clientes__3214EC073600C161");

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.NombreCompleto).HasMaxLength(120);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<DetallesPedido>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Detalles__3214EC078A073133");

            entity.ToTable("DetallesPedido");

            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Pedido).WithMany(p => p.DetallesPedidos)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Producto).WithMany(p => p.DetallesPedidos)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Domicilio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Domicili__3214EC07C94EDC7E");

            entity.HasIndex(e => e.EstadoDomicilio, "IX_Domicilios_EstadoDomicilio");

            entity.HasIndex(e => e.HoraSolicitud, "IX_Domicilios_HoraSolicitud");

            entity.Property(e => e.CostoDomicilio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Notas).HasMaxLength(250);
            entity.Property(e => e.Propina).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Pedido).WithMany(p => p.Domicilios).HasForeignKey(d => d.PedidoId);

            entity.HasOne(d => d.Repartidor).WithMany(p => p.Domicilios).HasForeignKey(d => d.RepartidorId);

            entity.HasOne(d => d.ZonaEntrega).WithMany(p => p.Domicilios).HasForeignKey(d => d.ZonaEntregaId);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pedidos__3214EC071392ABA8");

            entity.HasIndex(e => e.EstadoPedido, "IX_Pedidos_EstadoPedido");

            entity.HasIndex(e => e.FechaPedido, "IX_Pedidos_FechaPedido");

            entity.Property(e => e.MetodoPago).HasMaxLength(50);
            entity.Property(e => e.Observaciones).HasMaxLength(250);
            entity.Property(e => e.Total).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC07ADCF66C4");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Categoria).HasMaxLength(80);
            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.Nombre).HasMaxLength(120);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Repartidore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Repartid__3214EC0717595930");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Documento).HasMaxLength(50);
            entity.Property(e => e.NombreCompleto).HasMaxLength(120);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<ZonasEntrega>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ZonasEnt__3214EC07B0BC4DE9");

            entity.ToTable("ZonasEntrega");

            entity.Property(e => e.CostoDomicilioBase).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.NombreZona).HasMaxLength(80);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
