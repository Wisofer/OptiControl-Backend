using Microsoft.EntityFrameworkCore;
using OptiControl.Models.Entities;

namespace OptiControl.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ServiceOptica> ServiceOpticas { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<CajaDiaria> CajaDiaria { get; set; }
    public DbSet<AgencySettings> AgencySettings { get; set; }
    public DbSet<WhatsAppTemplate> WhatsAppTemplates { get; set; }
    public DbSet<WebsiteService> WebsiteServices { get; set; }
    public DbSet<Testimonial> Testimonials { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreUsuario).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Contrasena).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Rol).IsRequired().HasMaxLength(50);
            entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.NombreUsuario).IsUnique();
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Pasaporte).HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.GraduacionOd).HasMaxLength(50);
            entity.Property(e => e.GraduacionOi).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pendiente");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Destination).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(50).HasDefaultValue("Pendiente");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.HasOne(e => e.Client)
                .WithMany(c => c.Reservations)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Product).HasMaxLength(500);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ClientName).HasMaxLength(200);
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pagada");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.HasOne(e => e.Client)
                .WithMany(c => c.Sales)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(e => e.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreProducto).IsRequired().HasMaxLength(300);
            entity.Property(e => e.TipoProducto).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Marca).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(2000);
            entity.Property(e => e.Proveedor).HasMaxLength(200);
            entity.Property(e => e.PrecioCompra).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<ServiceOptica>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreServicio).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(20);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pendiente");
            entity.Property(e => e.Concept).HasMaxLength(500);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.HasOne(e => e.Client)
                .WithMany(c => c.Invoices)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.EntityId).HasMaxLength(50);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Concept).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50).HasDefaultValue("Operativo");
        });

        modelBuilder.Entity<CajaDiaria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Opening).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Sales).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Expenses).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Closing).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.Date).IsUnique();
        });

        modelBuilder.Entity<AgencySettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Language).HasMaxLength(10);
            entity.Property(e => e.ExchangeRate).HasColumnType("decimal(18,4)");
            entity.Property(e => e.Theme).HasMaxLength(20);
        });

        modelBuilder.Entity<WhatsAppTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Mensaje).IsRequired();
        });

        modelBuilder.Entity<WebsiteService>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ShortDescription).HasMaxLength(500);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Icon).HasMaxLength(100);
        });

        modelBuilder.Entity<Testimonial>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quote).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.AuthorName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(150);
        });
    }
}
