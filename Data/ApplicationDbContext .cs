using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using WebApiExamen.Models;

namespace WebApiExamen.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ArchivoCliente> ArchivosCliente { get; set; }
        public DbSet<LogApi> LogsApi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.CI);
                entity.Property(e => e.CI).HasMaxLength(20);
                entity.Property(e => e.Nombres).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Direccion).HasMaxLength(300).IsRequired();
                entity.Property(e => e.Telefono).HasMaxLength(20).IsRequired();

                // Configurar columnas de tipo varbinary(max) para las fotos
                entity.Property(e => e.FotoCasa1).HasColumnType("varbinary(max)");
                entity.Property(e => e.FotoCasa2).HasColumnType("varbinary(max)");
                entity.Property(e => e.FotoCasa3).HasColumnType("varbinary(max)");
            });

            // Configuración de ArchivoCliente
            modelBuilder.Entity<ArchivoCliente>(entity =>
            {
                entity.HasKey(e => e.IdArchivo);
                entity.Property(e => e.IdArchivo).ValueGeneratedOnAdd();

                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Archivos)
                    .HasForeignKey(e => e.CICliente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de LogApi
            modelBuilder.Entity<LogApi>(entity =>
            {
                entity.HasKey(e => e.IdLog);
                entity.Property(e => e.IdLog).ValueGeneratedOnAdd();
                entity.Property(e => e.TipoLog).HasMaxLength(50).IsRequired();
                entity.Property(e => e.MetodoHttp).HasMaxLength(10).IsRequired();
                entity.Property(e => e.UrlEndpoint).HasMaxLength(500).IsRequired();
                entity.Property(e => e.DireccionIp).HasMaxLength(50);
            });
        }
    }
}