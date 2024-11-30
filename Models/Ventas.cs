using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class Ventas
    {
        [Key]
        [Column("venta_id")]
        public int VentaId { get; set; }
        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;
        [Column("cliente_id")]
        public int? ClienteId { get; set; }
        [Column("empleado_id")]
        public int EmpleadoId { get; set; }
        [Column("total")]
        public decimal Total { get; set; }

        // Relaciones
        public Cliente? Cliente { get; set; }
        public Empleado? Empleado { get; set; }

        public ICollection<DetalleVenta> DetalleVentas { get; set; }
    }
}
