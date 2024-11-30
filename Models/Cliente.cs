using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        public string NIT { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellido { get; set; }

        [Required]
        public string Direccion { get; set; }

        [Required]
        public string Telefono { get; set; }

        public ICollection<Ventas>? Ventas { get; set; }
    }
}
