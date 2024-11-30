using Microsoft.EntityFrameworkCore;
using POS.Models;
using System.Collections.Generic;

namespace POS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
        
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Ventas> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVentas { get; set; }

        
    }
}
