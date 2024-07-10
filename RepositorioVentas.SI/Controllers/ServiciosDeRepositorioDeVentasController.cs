using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RepositorioVentas.Model;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Web.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RepositorioVentas.SI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiciosDeRepositorioDeVentasController : ControllerBase
    {
        private RepositorioVentas.DA.BDContexto Conexion;
        private RepositorioVentas.BL.Repositorio RegistroDeInventario;

        public ServiciosDeRepositorioDeVentasController(RepositorioVentas.DA.BDContexto conexion, IMemoryCache CacheAcumuladoCaja, IMemoryCache CacheUsuarioIniciado, IMemoryCache elCache)
        {

            Conexion = conexion;
            RegistroDeInventario = new RepositorioVentas.BL.Repositorio(conexion, CacheUsuarioIniciado, CacheUsuarioIniciado, elCache);
        }
        [HttpGet("ObtengaElInventario")]
        public List<Model.Inventario> ObtengaElInventario()
        {
            return RegistroDeInventario.ObtengaElInventario();
        }
        [HttpGet("ObtengaLosValoresDeCacheAcumulado")]
        public void ObtengaLosValoresDeCacheAcumulado()
        {
            RegistroDeInventario.ObtengaLosValoresDeCacheAcumulado();
        }

        [HttpGet("ElCacheEstaVacio")]
        public bool ElCacheEstaVacio()
        {
           return RegistroDeInventario.ElCacheEstaVacio();
        }
       

        [HttpGet("ObtengaLosValoresDelCache")]
        public void ObtengaLosValoresDelCache()
        {
           RegistroDeInventario.ObtengaLosValoresDelCache();

        }
       
        [HttpGet("CacheAcumuladoEstaVacio")]
       public bool CacheAcumuladoEstaVacio()
        {
           return RegistroDeInventario.CacheAcumuladoEstaVacio();
        }
        [HttpGet("ObtengaLosValoresDeCacheUsuarioIniciado")]
        public Model.Usuario ObtengaLosValoresDeCacheUsuarioIniciado()
        {
          return RegistroDeInventario.ObtengaLosValoresDeCacheUsuarioIniciado();
        }
        [HttpGet("Obtengausuario")]
        public string Obtengausuario()
        {

            return RegistroDeInventario.Obtengausuario();
        }
       
        [HttpGet("CacheUsuarioIniciadoEstaVacio")]
        public bool CacheUsuarioIniciadoEstaVacio()
        {
          return  RegistroDeInventario.CacheUsuarioIniciadoEstaVacio();
        }

        [HttpGet("EncontrarUsuarioPorNombre")]
        public Model.Usuario EncontrarUsuarioPorNombre(string nombre)
        {

            return RegistroDeInventario.EncontrarUsuarioPorNombre(nombre);
        }
        [HttpGet("ObtengaIdCaja")]
        public int ObtengaIdCaja()
        {
            return RegistroDeInventario.ObtengaIdCaja();
        }


        [HttpGet("ObtengaElAjusteDeInventario")]
        public List<Model.AjusteDeInventario> ObtengaElAjusteDeInventario()
        {
           return RegistroDeInventario.ObtengaElAjusteDeInventario();
           
        }

        [HttpGet("ObtengaLosAjustesPorInventario")]
        public List<Model.AjusteDeInventario> ObtengaLosAjustesPorInventario(int id)
        {
           
            return RegistroDeInventario.ObtengaLosAjustesPorInventario(id);
            
        }
        [HttpGet("ObtengaElAjustePorId")]
        public Model.AjusteDeInventario ObtengaElAjustePorId(int Id)
        {
            
            return RegistroDeInventario.ObtengaElAjustePorId( Id);
        }
        [HttpGet("ObtengaLaListaPorNombre")]
        public List<Model.Inventario> ObtengaLaListaPorNombre(string nombre)
        {
            
            return RegistroDeInventario.ObtengaLaListaPorNombre(nombre);
        }
        [HttpGet("ObtengaElInventarioPorId")]
        public Model.Inventario ObtengaElInventarioPorId(int Id)
        {
           
            return RegistroDeInventario.ObtengaElInventarioPorId( Id);
        }

        [HttpGet("EncontrarUsuario")]
        public Model.Usuario EncontrarUsuario(string nombre, string clave)
        {
            return RegistroDeInventario.EncontrarUsuario(nombre, clave);
        }
        [HttpGet("BuscarUsuarioPorId")]
        public Model.Usuario BuscarUsuarioPorId(Model.Usuario usuario)
        {
            return RegistroDeInventario.BuscarUsuarioPorId(usuario);
        }
        [HttpGet("BuscarUsuariosRegistrados")]
        public Model.Usuario BuscarUsuariosRegistrados(string nombre)
        {

            return RegistroDeInventario.BuscarUsuariosRegistrados(nombre);
        }
        [HttpGet("ObtenerAcumulado")]
        public AcumuladoCaja ObtenerAcumulado()
        {
            return RegistroDeInventario.ObtenerAcumulado();
        }

        [HttpPost("Edite")]
        public IActionResult Edite([FromBody] Model.Inventario inventario)
        {
            if (ModelState.IsValid)
            {
                RegistroDeInventario.Edite(inventario);
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
     
        [HttpGet("GenerarID")]
        public string GenerarID()
        {
            return RegistroDeInventario.GenerarID();
        }
       
        [HttpPut("ContarInicioDeSesionFallidos")]
        public Model.Usuario ContarInicioDeSesionFallidos([FromBody] Model.Usuario usuario)
        {
            return RegistroDeInventario.ContarInicioDeSesionFallidos(usuario);
        }
        [HttpPut("DesactivarBloqueo")]
        public Model.Usuario DesactivarBloqueo([FromBody]Model.Usuario usuario)
        {

            return RegistroDeInventario.DesactivarBloqueo(usuario);
        }


        [HttpPut("BloquearUsuario")]
        public Model.Usuario BloquearUsuario(Model.Usuario usuario)
        {
            return RegistroDeInventario.BloquearUsuario(usuario);
        }
        [HttpPost("EncriptarClave")]
        public string EncriptarClave(Model.Usuario usuario)
        {
            return RegistroDeInventario.EncriptarClave(usuario);
        }
        [HttpPut("EncriptarContrasena")]
        public string EncriptarContrasena([FromBody]string PasswordHash)
        {
            return RegistroDeInventario.EncriptarContrasena(PasswordHash);
        }

        [HttpPut("GenerarIdInt")]
        public int GenerarIdInt()
        {
          return RegistroDeInventario.GenerarIdInt();
        }

       
        [HttpPost("CierreAperturaCaja")]
        public void CierreAperturaCaja()
        {
            RegistroDeInventario.CierreAperturaCaja();

            }
        [HttpPut("GuardeUsuarioIniciado")]
        public IActionResult GuardeUsuarioIniciado(Model.Usuario Usuario)
        {
            if (ModelState.IsValid)
            {
                RegistroDeInventario.GuardeUsuarioIniciado(Usuario);
                return Ok();

            }
            else
            {
                return BadRequest(ModelState);
            }

        }

        [HttpPut("ResteEnElInventarioTrasVenta")]
        public IActionResult ResteEnElInventarioTrasVenta(int id_venta)
        {

            if (ModelState.IsValid)
            {
                RegistroDeInventario.ResteEnElInventarioTrasVenta(id_venta);
                return Ok();

            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpDelete("EliminarInventarioVenta")]

        public IActionResult EliminarInventarioVenta(int Identificador_Ventas)
        {
            RegistroDeInventario.EliminarInventarioVenta(Identificador_Ventas);
            return Ok();
        }

        [HttpPut("EstablecerUltimoIdVenta")]
        public void EstablecerUltimoIdVenta()
        {
            RegistroDeInventario.EstablecerUltimoIdVenta();
        }
        [HttpPut("ActualizarUsuario")]
        public Model.Usuario ActualizarUsuario(Model.Usuario usuariomodificado)
        {

            return RegistroDeInventario.ActualizarUsuario(usuariomodificado);
        }



        [HttpPost("CreeElCache")]
        public void CreeElCache()
        {
            RegistroDeInventario.CreeElCache();
        }
        [HttpPost("RegistreAperturaCaja")]
        public bool RegistreAperturaCaja()
        {
            return RegistroDeInventario.RegistreAperturaCaja();

        }
        [HttpPost("CreeCacheAcumulado")]
        public void CreeCacheAcumulado()
        {
            RegistroDeInventario.CreeCacheAcumulado();
        }
        [HttpPost("CreeCacheUsuarioIniciado")]
        public void CreeCacheUsuarioIniciado()
        {
            RegistroDeInventario.CreeCacheUsuarioIniciado();
        }
        [HttpPost("RegistreUsuarios")]
        public IActionResult RegistreUsuarios([FromBody] Model.Usuario usuario)
        {
            RegistroDeInventario.RegistreUsuarios(usuario);
            return Ok(usuario);
        }
        [HttpPost("RegistreInventario")]
        public IActionResult RegistreInventario([FromBody] Model.Inventario inventario)
        {

            RegistroDeInventario.RegistreInventario(inventario);
            return Ok(inventario);
        }
        [HttpPost("RegistreVenta")]
        public IActionResult RegistreVenta([FromBody] Model.Ventas ventas)
        {
            RegistroDeInventario.RegistreVenta(ventas);
          
           
            return Ok(ventas);

        }
        [HttpPost("RegistreInventarioVenta")]
        public IActionResult RegistreInventarioVenta([FromBody] Model.VentasInventario ventas)
        {
            if (ModelState.IsValid)
            {
                RegistroDeInventario.RegistreInventarioVenta(ventas);
                return Ok();

            }
            else
            {
                return BadRequest(ModelState);
            }



        }
        [HttpPost("EnviarEmailDeRestauracionDeClave")]
        public void EnviarEmailDeRestauracionDeClave([FromBody] Model.Usuario Usuario)
        {
            RegistroDeInventario.EnviarEmailDeRestauracionDeClave(Usuario);
        }
        [HttpPost("EnviarEmailDeUsuarioRegistrado")]
        public void EnviarEmailDeUsuarioRegistrado([FromBody]Model.Usuario Usuario)
        {
           RegistroDeInventario.EnviarEmailDeUsuarioRegistrado(Usuario);
        }
        [HttpPost("EnviarEmailDeInicioDeSesion")]
        public void EnviarEmailDeInicioDeSesion([FromBody]Model.Usuario Usuario)
        {
            RegistroDeInventario.EnviarEmailDeInicioDeSesion(Usuario);
        }
        [HttpPost("EnviarEmailBloqueoUsuario")]
        public async Task EnviarEmailBloqueoUsuario([FromBody]Model.Usuario usuario)
        {
          await RegistroDeInventario.EnviarEmailBloqueoUsuario(usuario);    
        }
        [HttpPost("EnviarEmailIntentoDeInicioDeSesionUsuarioBloqueado")]
        public async Task EnviarEmailIntentoDeInicioDeSesionUsuarioBloqueado([FromBody] Model.Usuario usuario)
        {
           await RegistroDeInventario.EnviarEmailIntentoDeInicioDeSesionUsuarioBloqueado(usuario);
        }
        [HttpPost("RegistreAjusteInventario")]
        public void RegistreAjusteInventario([FromBody]Model.AjusteDeInventario ajuste)
        {
            RegistroDeInventario.RegistreAjusteInventario(ajuste);  
        }
    }
    }
