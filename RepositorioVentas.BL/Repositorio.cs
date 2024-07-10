using System.Net.Mail;
using System.Net;
using MimeKit;
using System.Text;
using System.Security.Cryptography;
using RepositorioVentas.Model;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System.Net.Http.Headers;
using Org.BouncyCastle.Utilities;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Web.Helpers;


namespace RepositorioVentas.BL
{


    public class Repositorio
    {
        private RepositorioVentas.DA.BDContexto Conexion;

        private readonly IMemoryCache ElCache;


        public AcumuladoCaja acumuladoCaja = new AcumuladoCaja(); 
        public Usuario usuarioIniciado;
        AperturaDeCaja aperturaDeCaja;


        public IMemoryCache CacheAcumuladoCaja;
        public IMemoryCache CacheUsuarioIniciado;

        int idEnInt = 0;

        List<Model.VentasInventario> ListaVenta;

        public Repositorio(RepositorioVentas.DA.BDContexto conexion, IMemoryCache cacheAcumulado, IMemoryCache cacheUsuarioIniciado, IMemoryCache elCache)
        {
            Conexion = conexion;
            CacheAcumuladoCaja = cacheAcumulado;
            CacheUsuarioIniciado = cacheUsuarioIniciado;
            //
            ElCache = elCache;

            if (ElCacheEstaVacio())
            {
                CreeElCache();
            }
            else
            {
                ObtengaLosValoresDelCache();
            }

            if (CacheAcumuladoEstaVacio())
            {
                CreeCacheAcumulado();
            }
            else
            {
                ObtengaLosValoresDeCacheAcumulado();
            }

            if (CacheUsuarioIniciadoEstaVacio())
            {
                CreeCacheUsuarioIniciado();
            }
            else
            {
                ObtengaLosValoresDeCacheUsuarioIniciado();
            }

        }
          public void ObtengaLosValoresDeCacheAcumulado()
        {
            acumuladoCaja = (AcumuladoCaja) CacheAcumuladoCaja.Get("acumuladoCaja");
        }


        public bool ElCacheEstaVacio()
        {
            if (ElCache.Get("Lista") is null)
                return true;
            else
                return false;
        }

        public void CreeElCache()
        {
            ListaVenta = new List<Model.VentasInventario>();
            ElCache.Set("Lista", ListaVenta);
        }


        public void ObtengaLosValoresDelCache()
        {
            ListaVenta = (List<Model.VentasInventario>)ElCache.Get("Lista");

        }

        public void CreeCacheAcumulado()
        {
            CacheAcumuladoCaja.Set("acumuladoCaja", acumuladoCaja);
        }

        public bool CacheAcumuladoEstaVacio()
        {
            if (CacheAcumuladoCaja.Get("acumuladoCaja") is null)
                return true;
            else
                return false;
        }

        public Model.Usuario ObtengaLosValoresDeCacheUsuarioIniciado()
        {
           return usuarioIniciado = (Usuario)CacheUsuarioIniciado.Get("usuarioIniciado");
        }
        public string Obtengausuario()
        {
            usuarioIniciado = (Usuario)CacheUsuarioIniciado.Get("usuarioIniciado");
            return usuarioIniciado.Id;
        }

        public void CreeCacheUsuarioIniciado()
        {
            CacheUsuarioIniciado.Set("usuarioIniciado", usuarioIniciado);
        }

        public bool CacheUsuarioIniciadoEstaVacio()
        {
            if (CacheUsuarioIniciado.Get("usuarioIniciado") is null)
                return true;
            else
                return false;
        }

        public void GuardeUsuarioIniciado(Model.Usuario Usuario)
        {

            CacheUsuarioIniciado.Set("usuarioIniciado", Usuario);
            usuarioIniciado = Usuario;
            Console.WriteLine("entro");
        }
        
        public void RegistreUsuarios(Model.Usuario Usuario)
        {
            

            Conexion.AspNetUsers.Add(Usuario);
            Conexion.SaveChanges();
        }
       
        public void RegistreInventario(Model.Inventario inventario)
        {

            Conexion.Inventarios.Add(inventario);
            Conexion.SaveChanges();
        }

        public void RegistreVenta(Model.Ventas ventas)
        {

            ventas.MontoDescuento += ((ventas.Total * ventas.PorcentajeDesCuento) / 100);
            ventas.Total = (ventas.Total - (ventas.Total * ventas.PorcentajeDesCuento) / 100);

            if (ventas.TipoDePago == 1)
            {
                acumuladoCaja.Efectivo = acumuladoCaja.Efectivo + ventas.Total;
                Console.WriteLine("entro efectivo  " + (acumuladoCaja.Efectivo));
            }
            else if (ventas.TipoDePago == 2)
            {
                acumuladoCaja.Tarjeta = acumuladoCaja.Tarjeta + ventas.Total;
                Console.WriteLine("entro Tarjeta  " + (acumuladoCaja.Tarjeta));
            }
            else
            {
                acumuladoCaja.SINPEMovil = acumuladoCaja.SINPEMovil + ventas.Total;
                Console.WriteLine("entro simpe  " + (acumuladoCaja.SINPEMovil));
            }

            Conexion.Ventas.Add(ventas);
            Conexion.SaveChanges();

            EstablecerUltimoIdVenta();
        }

        public void ResteEnElInventarioTrasVenta(int id_venta) {

            // Cargar la lista actualizada desde el caché
            ListaVenta = ElCache.Get<List<Model.VentasInventario>>("Lista");

            foreach (var item in ListaVenta)
            {
                VentaDetalles ventaDetalles = new VentaDetalles();
                ventaDetalles.Id_Venta = id_venta;
                ventaDetalles.Id_Inventario = item.Id_Inventario;
                ventaDetalles.Cantidad = item.Cantidad;
                ventaDetalles.Precio = item.Precio;
                ventaDetalles.Monto = item.Monto;
                ventaDetalles.MontoDescuento = item.MontoDescuento;

                Conexion.VentaDetalles.Add(ventaDetalles);
                Conexion.SaveChanges();

                Inventario inventario = ObtengaElInventarioPorId(item.Id_Inventario);

                inventario.Cantidad -= item.Cantidad;

                // Actualizar el inventario en la base de datos
                Conexion.Inventarios.Update(inventario);
                Conexion.SaveChanges();
            }

            ListaVenta.Clear();
            ElCache.Set("Lista", ListaVenta);
        }

        // Registrar en la lista los productos del inventario de venta
        public void RegistreInventarioVenta(Model.VentasInventario ventas) {

            

            ListaVenta.Add(ventas);
            
        }

        // Registrar en la lista los productos del inventario de venta
        public void EliminarInventarioVenta(int Identificador_Ventas)
        {
            ListaVenta.RemoveAll(item => item.Identificador_Ventas == Identificador_Ventas);

            // Actualizar el caché después de eliminar elementos de la lista
            ElCache.Set("Lista", ListaVenta);
        }


        // Obtener Id de la ultima Venta
        public void EstablecerUltimoIdVenta()
        {
            var ultimaVenta = Conexion.Ventas.OrderByDescending(v => v.Id).FirstOrDefault();
            
            if (ultimaVenta != null)
            {
                ResteEnElInventarioTrasVenta(ultimaVenta.Id);
            }
            else
            {
                throw new InvalidOperationException("No se encontraron ventas registradas.");
            }
        }


        public void RegistreAjusteInventario(AjusteDeInventario ajuste)
        {
            // Obtener el inventario correspondiente al ajuste
            Inventario inventario = ObtengaElInventarioPorId(ajuste.Id_Inventario);

            if (ajuste.Tipo == 1) // Aumento
            {
                inventario.Cantidad += ajuste.Ajuste; // Aumentar la cantidad del inventario
            }
            else if (ajuste.Tipo == 2) // Disminución
            {
                if (inventario.Cantidad < ajuste.Ajuste)
                {
                    return; // Salir del método sin guardar los cambios
                }
                inventario.Cantidad -= ajuste.Ajuste; // Disminuir la cantidad del inventario
            }

            // Actualizar la cantidad actual del inventario en el ajuste
            ajuste.CantidadActual = inventario.Cantidad;

            // Establecer la fecha y el usuario automáticamente
            ajuste.Fecha = DateTime.Now;
            ajuste.Id = 0;

            // Guardar el ajuste en la base de datos
            Conexion.AjusteDeInventarios.Add(ajuste);
            Conexion.SaveChanges();

            // Actualizar el inventario en la base de datos
            Conexion.Inventarios.Update(inventario);
            Conexion.SaveChanges();
        }

        public List<Model.Inventario> ObtengaElInventario()
        {
            return Conexion.Inventarios.ToList();
        }

        public List<Model.AjusteDeInventario> ObtengaElAjusteDeInventario()
        {
            return Conexion.AjusteDeInventarios.ToList();
        }

        public List<Model.AjusteDeInventario> ObtengaLosAjustesPorInventario(int Id)
        {
            List<Model.AjusteDeInventario> listaDeAjustesSegunIdInventario;

            listaDeAjustesSegunIdInventario = Conexion.AjusteDeInventarios.Where(x => x.Id_Inventario == Id).ToList();

            return listaDeAjustesSegunIdInventario;
        }

        public Model.AjusteDeInventario ObtengaElAjustePorId(int Id)
        {
            foreach (var Producto in Conexion.AjusteDeInventarios)
            {
                if (Producto.Id == Id)
                { return Producto; }
            }
            return null;
        }

        public List<Model.Inventario> ObtengaLaListaPorNombre(string nombre)
        {
            List<Model.Inventario> listado_Del_Inventario_Segun_El_Nombre;

            listado_Del_Inventario_Segun_El_Nombre = Conexion.Inventarios.Where(x => x.Nombre.Contains(nombre)).ToList();

            return listado_Del_Inventario_Segun_El_Nombre;
        }

        public Model.Inventario ObtengaElInventarioPorId(int Id)
        {
            foreach (var Producto in Conexion.Inventarios)
            {
                if (Producto.Id == Id)
                { return Producto; }
            }
            return null;
        }


        public void Edite(Model.Inventario inventario)
        {
            Model.Inventario productoPorModificar;

            productoPorModificar = ObtengaElInventarioPorId(inventario.Id);

            productoPorModificar.Nombre = inventario.Nombre;
            productoPorModificar.Categoria = inventario.Categoria;
            productoPorModificar.Precio = inventario.Precio;


            Conexion.Inventarios.Update(productoPorModificar);
            Conexion.SaveChanges();
        }

        public void EnviarEmailDeUsuarioRegistrado(Model.Usuario Usuario)
        {
        string senderEmail = "lenguajesprueba2@gmail.com"; // Dirección de correo electrónico del remitente
        string senderPassword = "cujotnzioyjbmgzw"; // Contraseña de la cuenta de correo electrónico del remitente
        string recipientEmail = Usuario.Email; // Dirección de correo electrónico del destinatario
        string subject = "Solicitud de creación de usuario";
        string body = $"Cuenta de usuario creada satisfactoriamente para el usuario {Usuario.UserName}.";

        MailMessage mailMessage = new MailMessage(senderEmail, recipientEmail, subject, body);
        mailMessage.IsBodyHtml = false;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
        smtpClient.EnableSsl = true;

        try
        {
            smtpClient.Send(mailMessage);
        }
        catch (Exception ex)
        {
            // Manejar el error en el envío del correo electrónico
            Console.WriteLine("Error al enviar el correo electrónico: " + ex.Message);
        }
        }
        public void EnviarEmailDeInicioDeSesion(Model.Usuario Usuario)
        {
            string senderEmail = "lenguajesprueba2@gmail.com"; // Dirección de correo electrónico del remitente
            string senderPassword = "cujotnzioyjbmgzw"; // Contraseña de la cuenta de correo electrónico del remitente
            string recipientEmail = Usuario.Email; // Dirección de correo electrónico del destinatario
            string subject = "Inicio de sesión usuario " + Usuario.UserName;
            string body = "Usted inició sesión el día " + DateTime.Now.ToString("dd/MM/yyyy") + " a las " + DateTime.Now.ToString("HH:mm") + ".";

            MailMessage mailMessage = new MailMessage(senderEmail, recipientEmail, subject, body);
            mailMessage.IsBodyHtml = false;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
            smtpClient.EnableSsl = true;

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                // Manejar el error en el envío del correo electrónico
                Console.WriteLine("Error al enviar el correo electrónico: " + ex.Message);
            }
        }
        public async Task EnviarEmailBloqueoUsuario(Model.Usuario usuario)
        {
            string senderEmail = "lenguajesprueba2@gmail.com"; // Dirección de correo electrónico del remitente
            string senderPassword = "cujotnzioyjbmgzw"; // Contraseña de la cuenta de correo electrónico del remitente
            string recipientEmail = usuario.Email; // Dirección de correo electrónico del destinatario
            string subject = "Usuario Bloqueado";
            string body = $"Le informamos que la cuenta del usuario {usuario.UserName} se encuentra bloqueada por 10 minutos. Por favor, intente el día {usuario.LockoutEnd.Value.ToString("dd/MM/yyyy")} a las {usuario.LockoutEnd.Value.ToString("HH:mm:ss")}.";

            MailMessage mailMessage = new MailMessage(senderEmail, recipientEmail, subject, body);
            mailMessage.IsBodyHtml = false;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
            smtpClient.EnableSsl = true;

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Manejar el error en el envío del correo electrónico
                Console.WriteLine("Error al enviar el correo electrónico: " + ex.Message);
            }
        }
        public async Task EnviarEmailIntentoDeInicioDeSesionUsuarioBloqueado(Model.Usuario usuario)
        {
            string senderEmail = "lenguajesprueba2@gmail.com"; // Dirección de correo electrónico del remitente
            string senderPassword = "cujotnzioyjbmgzw"; // Contraseña de la cuenta de correo electrónico del remitente
            string recipientEmail = usuario.Email; // Dirección de correo electrónico del destinatario
            string subject = "Intento de inicio de sesión del usuario " + usuario.UserName + " bloqueado.";
            string body = $"Le informamos que la cuenta del usuario {usuario.UserName} se encuentra bloqueada por 10 minutos. Por favor, intente el día {usuario.LockoutEnd.Value.ToString("dd/MM/yyyy")} a las {usuario.LockoutEnd.Value.ToString("HH:mm:ss")}.";

            MailMessage mailMessage = new MailMessage(senderEmail, recipientEmail, subject, body);
            mailMessage.IsBodyHtml = false;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
            smtpClient.EnableSsl = true;

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Manejar el error en el envío del correo electrónico
                Console.WriteLine("Error al enviar el correo electrónico: " + ex.Message);
            }
        }

        public bool ValidarCredenciales(string UserName,string PasswordHash)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(PasswordHash);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                return Conexion.AspNetUsers.Any(u => u.UserName == UserName && u.PasswordHash == hash);
            }
        }
        public Model.Usuario Validar(string UserName, string PasswordHash)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(PasswordHash);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                return Conexion.AspNetUsers.FirstOrDefault(u => u.UserName == UserName && (u.PasswordHash ?? "") == hash);
            }
        }


        public string EncriptarClave(Model.Usuario usuario)
        {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(usuario.PasswordHash);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return hash;
        }
        }
        public string EncriptarContrasena(string PasswordHash)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(PasswordHash);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return hash;
            }
        }

        public Model.Usuario EncontrarUsuario(string nombre, string clave)
        {
            clave = EncriptarContrasena(clave);
            if (Conexion.AspNetUsers != null)
            {
                var usuarioEncontrado = Conexion.AspNetUsers
                    .Where(item => item.UserName == nombre && item.PasswordHash == clave)
                    .Select(item => new Model.Usuario
                    {
                        // Aquí seleccionas los valores que deseas obtener del objeto AspNetUsers
                        UserName = item.UserName,
                        PasswordHash = item.PasswordHash,
                        Id = item.Id,
                        Email = item.Email
                        
                    })
                    .SingleOrDefault();

                return usuarioEncontrado;
            }

            return null;
        }
        public Model.Usuario BuscarUsuarioPorId(Model.Usuario usuario)
        {
            var usuarioBuscado = Conexion.AspNetUsers.FirstOrDefault(u => u.Id == usuario.Id);
            return usuarioBuscado;
        }
        public Model.Usuario BuscarUsuariosRegistrados(string nombre)
        {

            if (Conexion.AspNetUsers != null)
            {
                var usuarioEncontrado = Conexion.AspNetUsers
                    .Where(item => item.UserName == nombre)
                    .Select(item => new Model.Usuario
                    {

                        UserName = item.UserName


                    })
                    .SingleOrDefault();

                return usuarioEncontrado;
            }

            return null;
        }
        public string GenerarID()
        {
            const string caracteresPermitidos = "abcdefghijklmnopqrstuvwxyz0123456789";
            int longitudID = 4;

            StringBuilder sb = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < longitudID; i++)
            {
                int indice = random.Next(caracteresPermitidos.Length);
                sb.Append(caracteresPermitidos[indice]);
            }

            return sb.ToString();
        }


        public bool VerificarClave(Model.Usuario usuario, string claveIngresada)
        {
            string claveEncriptadaIngresada = EncriptarClave(new Usuario { PasswordHash = claveIngresada });
            return string.Equals(usuario.PasswordHash, claveEncriptadaIngresada, StringComparison.OrdinalIgnoreCase);
        }
        public void EnviarEmailDeRestauracionDeClave(Model.Usuario Usuario)
        {
            string senderEmail = "lenguajesprueba2@gmail.com"; // Dirección de correo electrónico del remitente
            string senderPassword = "cujotnzioyjbmgzw"; // Contraseña de la cuenta de correo electrónico del remitente generada en gmail por medio de la funcion generar contrasenas para aplicaciones terceras porque gmail cambio las politicas de seguridad
            string recipientEmail = Usuario.Email; // Dirección de correo electrónico del destinatario
            string subject = "Cambio de clave " + Usuario.UserName;
            string body = "Le informamos que el cambio de clave de la cuenta del usuario " + Usuario.UserName + " se ejecutó satisfactoriamente.";

            MailMessage mailMessage = new MailMessage(senderEmail, recipientEmail, subject, body);
            mailMessage.IsBodyHtml = false;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
            smtpClient.EnableSsl = true;

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine("Error al enviar el correo electrónico: " + ex.Message);
            }
        }

        public Model.Usuario ContarInicioDeSesionFallidos(Model.Usuario usuario)
        {
 
            usuario.AccessFailedCount++; 
            Conexion.Update(usuario);
            Conexion.SaveChanges();
            return usuario;
        }
        public Model.Usuario DesactivarBloqueo(Model.Usuario usuario)
        {

            usuario.LockoutEnabled = false;
            usuario.AccessFailedCount = 0;
            usuario.LockoutEnd = null;
            Conexion.Update(usuario);
            Conexion.SaveChanges();
            return usuario;
        }


        public Model.Usuario EncontrarUsuarioPorNombre(string nombre)
        {
           
            if (Conexion.AspNetUsers != null)
            {
                var usuarioEncontrado = Conexion.AspNetUsers
                    .Where(item => item.UserName == nombre )
                    .Select(item => new Model.Usuario
                    {
                       
                        UserName = item.UserName,
                        Id = item.Id,
                        AccessFailedCount = item.AccessFailedCount,
                        LockoutEnabled = item.LockoutEnabled,
                        PasswordHash = item.PasswordHash,
                        LockoutEnd = item.LockoutEnd,
                        Email = item.Email


                    })
                    .SingleOrDefault();

                return usuarioEncontrado;
            }

            return null;
        }
        public Model.Usuario ActualizarUsuario(Model.Usuario usuariomodificado)
        {
          
            Model.Usuario usuario = new Usuario();

            usuario = EncontrarUsuarioPorNombre(usuariomodificado.UserName);
            if (usuario != null)
            {
                usuario.PasswordHash = usuariomodificado.PasswordHash;
                Conexion.Update(usuario);
                Conexion.SaveChanges();
                return usuario;
            }
            else
            {
                return null;
            }

        }

        
        
        public int ObtengaIdCaja()
        {

            if (Conexion.AperturasDeCaja != null)
            {
                var cajaAbierta = Conexion.AperturasDeCaja
                    .Where(item => item.Estado == Estado.Abierta)
                    .Select(item => new Model.AperturaDeCaja
                    {

                       
                        Id = item.Id,
                       


                    })
                    .SingleOrDefault();

                return cajaAbierta.Id;
            }

            return 0;
        }
        public Model.Usuario BloquearUsuario(Model.Usuario usuario)
        {
            var usuarioBuscado = Conexion.AspNetUsers.FirstOrDefault(u => u.Id == usuario.Id);

            if (usuarioBuscado != null)
            {
                if (usuarioBuscado.LockoutEnd == null || usuarioBuscado.LockoutEnd < DateTimeOffset.Now)
                {
                    usuarioBuscado.LockoutEnabled = true; 

                    if (usuarioBuscado.LockoutEnd != null && usuarioBuscado.LockoutEnd > DateTimeOffset.Now)
                    {
                        
                        usuarioBuscado.LockoutEnd = usuarioBuscado.LockoutEnd;
                    }
                    else
                    {
                        
                        usuarioBuscado.LockoutEnd = DateTimeOffset.Now.AddMinutes(10);
                    }
                    Conexion.Update(usuarioBuscado);
                    Conexion.SaveChanges();
                }

                return usuarioBuscado;
            }

            return null;
        }


        public AcumuladoCaja ObtenerAcumulado()
        {
            return acumuladoCaja;
        }

        public int GenerarIdInt()
        {
            int min = 1;
            int max = 99999;

            Random rnd = new Random();
            int random = rnd.Next(min, max);

            return random;
        }

        public bool RegistreAperturaCaja()
        {
            bool abrioCaja = false;

            if (usuarioIniciado != null)
            {
                aperturaDeCaja = new AperturaDeCaja();

                aperturaDeCaja.UserId = usuarioIniciado.Id;
                aperturaDeCaja.FechaDeInicio = DateTime.Now;
                aperturaDeCaja.Estado = Model.Estado.Abierta;

                Conexion.AperturasDeCaja.Add(aperturaDeCaja);
                Conexion.SaveChanges();

                acumuladoCaja.FechaDeInicio = DateTime.Now;
               acumuladoCaja.Activo = true;
                abrioCaja = true;
            }
            return abrioCaja;

        }

        public void CierreAperturaCaja()
        {

            AperturaDeCaja AperturaCaja = new AperturaDeCaja();

            Console.WriteLine("entro cierreCaja");

            if (Conexion.AperturasDeCaja != null)
            {
                var aperturaEncontrada = Conexion.AperturasDeCaja
                    .Where(item => item.UserId == usuarioIniciado.Id)
                    .OrderByDescending(item => item.Id)
                    .Select(item => new Model.AperturaDeCaja
                    {
                        Id = item.Id,
                        UserId = item.UserId,
                        FechaDeInicio = item.FechaDeInicio,
                        
                    })
                    .FirstOrDefault();

                AperturaCaja.Id = aperturaEncontrada.Id;
                AperturaCaja.UserId = aperturaEncontrada.UserId;
                AperturaCaja.FechaDeInicio = aperturaEncontrada.FechaDeInicio;
                AperturaCaja.Estado = Model.Estado.Cerrada;
                AperturaCaja.FechaDeCierre = DateTime.Now;

                Conexion.AperturasDeCaja.Update(AperturaCaja);
                Conexion.SaveChanges();

                Console.WriteLine("actualizo");

                acumuladoCaja = new AcumuladoCaja();
                CreeCacheAcumulado();

            }

            

        }
        

    }
}

