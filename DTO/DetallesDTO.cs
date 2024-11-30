using POS.Models;

namespace POS.DTO
{
    public class DetallesDTO
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
