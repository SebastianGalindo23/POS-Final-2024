using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using POS.Data;
using POS.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using BCrypt.Net;

namespace POS.Controllers
{
    public class CuentaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CuentaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login() => View();

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Login(EmpleadoLoginModel model)
        {
            if (ModelState.IsValid)
            {
                var empleado = _context.Empleados.FirstOrDefault(e => e.Usuario == model.Usuario);
                if (empleado != null && BCrypt.Net.BCrypt.Verify(model.Contrasena, empleado.Contrasena))
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, empleado.Nombre),
                new Claim("EmpleadoId", empleado.Id.ToString()), 
                new Claim("Usuario", empleado.Usuario),
                new Claim(ClaimTypes.Role, empleado.Rol),
                new Claim("UltimaRuta", empleado.Rol == "Admin" ? "/Admin/Index" : "/Ventas/Index")
            };

                    var identity = new ClaimsIdentity(claims, "CookieAuth");
                    var principal = new ClaimsPrincipal(identity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    await HttpContext.SignInAsync("CookieAuth", principal, authProperties);

                    return empleado.Rol == "Admin"
                        ? RedirectToAction("Index", "Admin")
                        : RedirectToAction("Index", "Ventas");
                }

                ModelState.AddModelError("", "Usuario o contraseña incorrectos");
            }
            return View(model);
        }


        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(EmpleadoRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Contrasena);
                var empleado = new Empleado
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    DPI = model.DPI,
                    Cargo = model.Cargo,
                    Usuario = model.Usuario,
                    Contrasena = hashedPassword,
                    Rol = model.Rol 
                };

                _context.Empleados.Add(empleado);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth"); 
            return RedirectToAction("Login"); 
        }
    }
}
