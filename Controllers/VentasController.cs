using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using POS.Data;
using POS.Models;
using System.Reflection.Metadata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using POS.DTO;
using Document = QuestPDF.Fluent.Document;
using System.Security.Claims;



namespace POS.Controllers
{
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Index(string? search, string? filter)
        {
            var productos = _context.Productos.AsQueryable();

            
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(filter))
            {
                switch (filter.ToLower())
                {
                    case "nombre":
                        productos = productos.Where(p => p.Nombre.Contains(search));
                        break;

                    case "codigo":
                        productos = productos.Where(p => p.Codigo.Contains(search));
                        break;

                    case "precio":
                        if (decimal.TryParse(search, out var precio))
                        {
                            productos = productos.Where(p => p.Precio == precio);
                        }
                        break;

                    default:
                        break;
                }
            }
            ViewBag.Clientes = _context.Clientes.ToList();
            var listaProductos = productos.ToList();
            return View(listaProductos);
        }

        public IActionResult ImprimirFactura(long id)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            // Obtener la venta junto con los detalles
            var venta = _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.DetalleVentas)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefault(v => v.VentaId == id);

            if (venta == null)
            {
                return NotFound();
            }

            
            var empleadoNombre = User.Identity.IsAuthenticated
                ? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                : "Empleado desconocido";

            var monedaGuatemala = new System.Globalization.CultureInfo("es-GT");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);

                    page.Header().BorderBottom(1).PaddingBottom(10).Row(header =>
                    {
                        header.RelativeItem().Column(column =>
                        {
                            column.Item().Text("Punto de Venta").FontSize(20).Bold();
                            column.Item().Text("Dirección: Calle Ejemplo, Ciudad de Guatemala").FontSize(10).Italic();
                            column.Item().Text("Tel: +502 1234-5678").FontSize(10).Italic();
                        });

                    });

                    page.Content().Column(column =>
                    {
                        // Información general
                        column.Item().PaddingVertical(10).Row(row =>
                        {
                            row.RelativeItem().Text($"Fecha: {venta.Fecha:dd/MM/yyyy}").FontSize(12).Italic();
                            row.RelativeItem().Text($"Factura No.: {venta.VentaId}").FontSize(12).Italic();
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Cliente: {venta.Cliente?.Nombre ?? "Consumidor Final (CF)"}").FontSize(12).Bold();
                            row.RelativeItem().Text($"NIT: {venta.Cliente?.NIT ?? "CF"}").FontSize(12).Bold();
                        });

                        column.Item().Text($"Atendido por: {empleadoNombre}").FontSize(12).Italic();

                        column.Item().Text("").FontSize(12).Italic();

                        // Divider

                        // Tabla de productos
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1); 
                                columns.RelativeColumn(1); 
                                columns.RelativeColumn(1); 
                            });

                            // Encabezados
                            table.Header(header =>
                            {
                                header.Cell().Element(EstiloCelda).Text("Producto").FontSize(12).Bold();
                                header.Cell().Element(EstiloCelda).Text("Cantidad").FontSize(12).Bold();
                                header.Cell().Element(EstiloCelda).Text("Precio Unitario").FontSize(12).Bold();
                                header.Cell().Element(EstiloCelda).Text("Subtotal").FontSize(12).Bold();
                            });

                            // Detalles de productos
                            foreach (var detalle in venta.DetalleVentas)
                            {
                                table.Cell().Element(EstiloCeldaContenido).Text(detalle.Producto?.Nombre ?? "N/A");
                                table.Cell().Element(EstiloCeldaContenido).AlignCenter().Text(detalle.Cantidad.ToString());
                                table.Cell().Element(EstiloCeldaContenido).AlignRight().Text(detalle.PrecioUnitario.ToString("C", monedaGuatemala));
                                table.Cell().Element(EstiloCeldaContenido).AlignRight().Text((detalle.Cantidad * detalle.PrecioUnitario).ToString("C", monedaGuatemala));
                            }

                            // Total de la venta
                            table.Cell().ColumnSpan(3).Element(EstiloCeldaTotal).Text("TOTAL").FontSize(12).Bold().AlignRight();
                            table.Cell().Element(EstiloCeldaTotal).Text(venta.Total.ToString("C", monedaGuatemala)).FontSize(12).Bold().AlignRight();

                            // Estilos de celdas
                            static IContainer EstiloCelda(IContainer container)
                            {
                                return container.Background("#e8e8e8").Padding(5).Border(1).BorderColor("#007BFF").AlignCenter();
                            }

                            static IContainer EstiloCeldaContenido(IContainer container)
                            {
                                return container.Padding(5).Border(1).BorderColor("#007BFF");
                            }

                            static IContainer EstiloCeldaTotal(IContainer container)
                            {
                                return container.Background("#f5f5f5").Padding(5).Border(1).BorderColor("#007BFF");
                            }
                        });

                        column.Item().Text("").FontSize(12).Italic();

                        // Nota de agradecimiento
                        column.Item().AlignCenter().PaddingTop(10).Text("Gracias por su compra").FontSize(14).Bold();
                    });

                    // Pie de página
                    page.Footer().PaddingTop(10).Text(text =>
                    {
                        text.Span("Factura generada por Sistema POS. ").FontSize(10);
                        text.Span("© 2024 Todos los derechos reservados.").FontSize(10).Italic();
                    });
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"Factura_{id}.pdf");
        }

        // Verificar si una venta existe
        private bool VentaExists(int id)
        {
            return _context.Ventas.Any(e => e.VentaId == id);
        }

        [HttpPost]
        public async Task<IActionResult> CrearVenta([FromBody] VentasDTO ventaDto)
        {
            if (ventaDto == null || ventaDto.Detalles == null || !ventaDto.Detalles.Any())
            {
                return BadRequest("Datos de la venta inválidos");
            }

            var cliente = await _context.Clientes.FindAsync(ventaDto.ClienteId);
            if (cliente == null)
            {
                return BadRequest("El cliente seleccionado no existe");
            }

            var empleadoIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmpleadoId")?.Value;

            if (string.IsNullOrEmpty(empleadoIdClaim) || !int.TryParse(empleadoIdClaim, out int empleadoId))
            {
                return Unauthorized("Empleado no autenticado.");
            }

            var nuevaVenta = new Ventas
            {
                Fecha = DateTime.Now,
                ClienteId = ventaDto.ClienteId,
                EmpleadoId = empleadoId,
                Total = ventaDto.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)
            };

            _context.Ventas.Add(nuevaVenta);
            await _context.SaveChangesAsync();

            foreach (var detalleDto in ventaDto.Detalles)
            {
                var detalle = new DetalleVenta
                {
                    VentaId = nuevaVenta.VentaId,
                    ProductoId = detalleDto.ProductoId,
                    Cantidad = detalleDto.Cantidad,
                    PrecioUnitario = detalleDto.PrecioUnitario
                };
                _context.DetallesVentas.Add(detalle);

                var producto = await _context.Productos.FindAsync(detalleDto.ProductoId);
                if (producto != null)
                {
                    // Restar la cantidad vendida del stock
                    if (producto.Stock >= detalleDto.Cantidad)
                    {
                        producto.Stock -= detalleDto.Cantidad;
                    }
                    else
                    {
                        return BadRequest($"No hay suficiente stock para el producto {producto.Nombre}");
                    }
                }
                else
                {
                    return BadRequest($"El producto con ID {detalleDto.ProductoId} no existe");
                }
            }
            await _context.SaveChangesAsync();

            return Ok(new { VentaId = nuevaVenta.VentaId });
        }
    



    }
}

