using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    [Table("detalle_ventas")]
    public class DetalleVenta
    {
        [Column("detalle_venta_id")]
        public int DetalleVentaId { get; set; }
        [Column("venta_id")]
        public int VentaId { get; set; }
        [Column("producto_id")]
        public int ProductoId { get; set; }
        [Column("cantidad")]
        public int Cantidad { get; set; }
        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }
        [Column("subtotal")]
        public decimal Subtotal { get; set; }

        // Relaciones
        public Ventas Venta { get; set; }
        public Producto Producto { get; set; }
    }
}
