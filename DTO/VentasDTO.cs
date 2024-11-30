namespace POS.DTO
{
    public class VentasDTO
    {
        public int ClienteId { get; set; }
        public List<DetallesDTO> Detalles { get; set; }
    }
}
