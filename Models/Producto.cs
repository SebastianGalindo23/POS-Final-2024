using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El código solo debe contener números")]
        public string Codigo { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder los 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no debe exceder los 500 caracteres")]
        public string Descripcion { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser un número positivo")]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser un número entero no negativo")]
        public int Stock { get; set; }

        public string? UrlImagen { get; set; }

        public ICollection<DetalleVenta>? DetalleVentas { get; set; }
    }
}
