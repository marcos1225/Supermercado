
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using static System.Net.WebRequestMethods;

namespace RepositorioVentas.UI.Controllers
{
    public class UsuariosController : Controller
    {
        private RepositorioVentas.UI.BDContexto.BDContexto Conexion;
        private RepositorioVentas.UI.Data.Repositorio RegistroDeInventario;

        public UsuariosController(RepositorioVentas.UI.BDContexto.BDContexto conexion, IMemoryCache CacheAcumuladoCaja, IMemoryCache CacheUsuarioIniciado, IMemoryCache elCache)
        {
            Conexion = conexion;
            RegistroDeInventario = new RepositorioVentas.UI.Data.Repositorio(conexion, CacheUsuarioIniciado, CacheUsuarioIniciado, elCache);
        }

        // GET: UsuariosController
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(Models.Usuario usuariologueado)
        {
            try
            {
                  
                using (HttpClient httpClient = new HttpClient())
                {
                    // Encontrar Usuario                  
                    var url = $"https://apicomercioapi.azure-api.net/api/Comercio/EncontrarUsuario?nombre={Uri.EscapeDataString(usuariologueado.UserName)}&clave={Uri.EscapeDataString(usuariologueado.PasswordHash)}";
                    var response = await httpClient.GetAsync(url);


                    if (response.IsSuccessStatusCode)
                    {
                        var usuario = JsonConvert.DeserializeObject<Models.Usuario>(await response.Content.ReadAsStringAsync());

                        if (usuario != null && usuario.LockoutEnabled == false)
                        {
                            usuario.PhoneNumber = "0";
                            usuario.SecurityStamp = "null";
                            usuario.NormalizedEmail = usuario.Email;
                            usuario.ConcurrencyStamp = "null";
                            usuario.NormalizedUserName = usuario.UserName;
                            //enviar email
                            var enviarEmailUsuarioIniciadoUrl = "https://apicomercioapi.azure-api.net/api/Comercio/EnviarEmailDeInicioDeSesion";
                            var enviarEmailUsuarioRegistradoContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");
                            var enviarEmailResponse = await httpClient.PostAsync(enviarEmailUsuarioIniciadoUrl, enviarEmailUsuarioRegistradoContent);

                            if (enviarEmailResponse.IsSuccessStatusCode)
                            {
                                // Guardar Usuario Iniciado
                                var guardeUsuarioIniciadoUrl = $"https://apicomercioapi.azure-api.net/api/Comercio/GuardeUsuarioIniciado";
                                var guardeUsuarioIniciadoContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");
                                await httpClient.PutAsync(guardeUsuarioIniciadoUrl, guardeUsuarioIniciadoContent);

                                
                                return RedirectToAction("Index", "Inventario");
                            }
                            else
                            {
                                // Error al enviar el correo electrónico
                                var errorResponse = await enviarEmailResponse.Content.ReadAsStringAsync();
                                ModelState.AddModelError(string.Empty, "Ocurrió un error al enviar el correo electrónico: " + errorResponse);
                                return View();
                            }
                        }
                        else
                        {
                            Models.Usuario usuarioFallido = new Models.Usuario();
                            // Usuario fallido
                            var usuarioFallidoUrl = $"https://apicomercioapi.azure-api.net/api/Comercio/EncontrarUsuarioPorNombre?nombre={Uri.EscapeDataString(usuariologueado.UserName)}";
                            var usuarioFallidoResponse = await httpClient.GetAsync(usuarioFallidoUrl);
                          

                            if (usuarioFallidoResponse.IsSuccessStatusCode)
                            {
                               usuarioFallido = JsonConvert.DeserializeObject<Models.Usuario>(await usuarioFallidoResponse.Content.ReadAsStringAsync());

                                if (usuarioFallido != null)
                                {
                                    usuarioFallido.PhoneNumber = "0";
                                    usuarioFallido.SecurityStamp = "null";
                                    usuarioFallido.NormalizedEmail = usuarioFallido.Email;
                                    usuarioFallido.ConcurrencyStamp = "null";
                                    usuarioFallido.NormalizedUserName = usuarioFallido.UserName;
                                    // Contar Inicio de Sesión Fallidos
                                    var contarInicioSesionFallidosUrl = $"https://apicomercioapi.azure-api.net/api/Comercio/ContarInicioDeSesionFallidos";
                                    var contarInicioSesionFallidosContent = new StringContent(JsonConvert.SerializeObject(usuarioFallido), Encoding.UTF8, "application/json");
                                    var contarInicioSesionFallidosResponse = await httpClient.PutAsync(contarInicioSesionFallidosUrl, contarInicioSesionFallidosContent);
                                 

                                }
                                var usuarioFallidosUrl = $"https://apicomercioapi.azure-api.net/api/Comercio/EncontrarUsuarioPorNombre?nombre={Uri.EscapeDataString(usuarioFallido.UserName)}";
                                var usuarioFallidosResponse = await httpClient.GetAsync(usuarioFallidosUrl);
                              var  usuarioInusual = JsonConvert.DeserializeObject<Models.Usuario>(await usuarioFallidosResponse.Content.ReadAsStringAsync());

                                if (usuarioInusual != null && usuarioInusual.AccessFailedCount == 3)
                                {
                                    // Bloquear Usuario
                                    var bloquearUsuarioUrl = $"https://apicomercioapi.azure-api.net/api/Comercio/BloquearUsuario";
                                    var bloquearUsuarioContent = new StringContent(JsonConvert.SerializeObject(usuarioFallido), Encoding.UTF8, "application/json");
                                   var bloqueo = await httpClient.PutAsync(bloquearUsuarioUrl, bloquearUsuarioContent);

                                    var usuariobloqueado = JsonConvert.DeserializeObject<Models.Usuario>(await bloqueo.Content.ReadAsStringAsync());


                                    string errorMessage = $"El usuario se encuentra bloqueado, por favor vuelva a intentarlo el {usuariobloqueado.LockoutEnd:dd/MM/yyyy} a las {usuariobloqueado.LockoutEnd:HH:mm:ss}";
                                    // Enviar Email de Bloqueo de Usuario
                                    var enviarEmailIntentoInicioSesionUsuarioBloqueadoUrl = $"https://apicomercioapi.azure-api.net/api/Comercio/EnviarEmailBloqueoUsuario";
                                    var enviarEmailIntentoInicioSesionUsuarioBloqueadoContent = new StringContent(JsonConvert.SerializeObject(usuariobloqueado), Encoding.UTF8, "application/json");
                                   var enviaremailusuariobloqueado = await httpClient.PostAsync(enviarEmailIntentoInicioSesionUsuarioBloqueadoUrl, enviarEmailIntentoInicioSesionUsuarioBloqueadoContent);
                                    if (!enviaremailusuariobloqueado.IsSuccessStatusCode)
                                    {
                                        // Error al enviar el correo electrónico
                                        var errorResponse = await enviaremailusuariobloqueado.Content.ReadAsStringAsync();
                                        ModelState.AddModelError(string.Empty, "Ocurrió un error al enviar el correo electrónico: " + errorResponse);
                                        return View();
                                    }

                                    ModelState.AddModelError(string.Empty, errorMessage);
                                    return View();
                                }
                                else
                                {
                                    TimeZoneInfo zonaHorariaCR = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                                    DateTimeOffset fechaHoraCR = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, zonaHorariaCR);

                                    if (usuarioInusual != null && usuarioInusual.AccessFailedCount > 3 && usuarioInusual.LockoutEnabled == true)
                                    {
                                        string lockoutEndFormatted = usuarioFallido.LockoutEnd.Value.ToString("dd/MM/yyyy HH:mm:ss");
                                        DateTimeOffset lockoutEnd = DateTimeOffset.ParseExact(lockoutEndFormatted, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                        TimeSpan offset = new TimeSpan(-6, 0, 0); // Establecer desplazamiento -06:00
                                        usuarioInusual = usuarioFallido;
                                        usuarioInusual.LockoutEnd = new DateTimeOffset(lockoutEnd.DateTime, offset);

                                        if (usuarioInusual.LockoutEnd != null && usuarioInusual.LockoutEnd <= fechaHoraCR)
                                        {
                                            
                                            // Desactivar Bloqueo
                                            var desactivarBloqueoUrl = $"https://apicomercioapi.azure-api.net/api/Comercio/DesactivarBloqueo";
                                            var desactivarBloqueoContent = new StringContent(JsonConvert.SerializeObject(usuarioInusual), Encoding.UTF8, "application/json");
                                            var respuestaDesactivarBloqueo = await httpClient.PutAsync(desactivarBloqueoUrl, desactivarBloqueoContent);

                                            if (!respuestaDesactivarBloqueo.IsSuccessStatusCode)
                                            {
                                                // Error al enviar el correo electrónico
                                                var errorResponse = await respuestaDesactivarBloqueo.Content.ReadAsStringAsync();
                                                ModelState.AddModelError(string.Empty, "Ocurrió un error al enviar el correo electrónico: " + errorResponse);
                                                return View();
                                            }else
                                            {
                                                string errorMessagess = $"Por favor verifique las credenciales nuevamente";

                                                ModelState.AddModelError(string.Empty, errorMessagess);
                                                return View();
                                            }
                                          
                                        }




                                        // Enviar Email de Intento de Inicio de Sesión en Usuario Bloqueado
                                        var enviarEmailIntentoInicioSesionUsuarioBloqueadoUrl = $"https://apicomercioapi.azure-api.net/api/Comercio/EnviarEmailIntentoDeInicioDeSesionUsuarioBloqueado";
                                        var enviarEmailIntentoInicioSesionUsuarioBloqueadoContent = new StringContent(JsonConvert.SerializeObject(usuarioFallido), Encoding.UTF8, "application/json");
                                        await httpClient.PostAsync(enviarEmailIntentoInicioSesionUsuarioBloqueadoUrl, enviarEmailIntentoInicioSesionUsuarioBloqueadoContent);

                                        string errorMessages = $"El usuario se encuentra bloqueado, por favor vuelva a intentarlo el {usuarioFallido.LockoutEnd:dd/MM/yyyy} a las {usuarioFallido.LockoutEnd:HH:mm:ss}";

                                        ModelState.AddModelError(string.Empty, errorMessages);
                                        return View();
                                    }

                                    string errorMessage = "Nombre o usuario incorrectos, por favor verifique";

                                    ModelState.AddModelError(string.Empty, errorMessage);
                                    return View();
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Error al encontrar el usuario, por favor verifique");
                                return View();
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error al encontrar el usuario");
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error en la aplicación: {ex.Message}");
                return View();
            }
        }


        // GET: UsuariosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UsuariosController/Create
        public ActionResult Create()
        {
            
            return View();
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Models.Usuario usuario)
        {
            try
            {
                usuario.PhoneNumber = "0";
                usuario.SecurityStamp = "null";
                usuario.NormalizedEmail = usuario.Email;
                usuario.ConcurrencyStamp = "null";
                usuario.NormalizedUserName = usuario.UserName;
                var httpClient = new HttpClient();

                // Verificar si el nombre de usuario ya está en uso
                var buscarUsuarioUrl = $"https://apicomercioapi.azure-api.net/api/Comercio/BuscarUsuariosRegistrados?nombre={Uri.EscapeDataString(usuario.UserName)}";
                var buscarUsuarioResponse = await httpClient.GetAsync(buscarUsuarioUrl);
                var usuarioBuscado = JsonConvert.DeserializeObject<Models.Usuario>(await buscarUsuarioResponse.Content.ReadAsStringAsync());

                if (usuarioBuscado != null)
                {
                    ModelState.AddModelError(string.Empty, "Nombre de usuario ya está en uso, por favor intente con otro");
                    return View();
                }
                else
                {
                    // Generar ID
                    var generarIdUrl = "https://apicomercioapi.azure-api.net/api/Comercio/GenerarID";
                    var generarIdResponse = await httpClient.GetAsync(generarIdUrl);
                    var idGenerado = await generarIdResponse.Content.ReadAsStringAsync();

                    usuario.Id = idGenerado;

                    // Encriptar la contraseña
                    var generarclaveUrl = "https://apicomercioapi.azure-api.net/api/Comercio/EncriptarContrasena";
                    var generarclaveContent = new StringContent(JsonConvert.SerializeObject(usuario.PasswordHash), Encoding.UTF8, "application/json");
                    var generarclaveResponse = await httpClient.PutAsync(generarclaveUrl, generarclaveContent);
                    var claveEncriptada = await generarclaveResponse.Content.ReadAsStringAsync();

                    usuario.PasswordHash = claveEncriptada;

                    // Registrar el usuario
                    var registrarUsuarioUrl = "https://apicomercioapi.azure-api.net/api/Comercio/RegistreUsuarios";
                    var registrarUsuarioResponse = await httpClient.PostAsync(registrarUsuarioUrl, new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json"));

                    if (registrarUsuarioResponse.IsSuccessStatusCode)
                    {
                        // Enviar email de usuario registrado
                        var enviarEmailUsuarioRegistradoUrl = "https://apicomercioapi.azure-api.net/api/Comercio/EnviarEmailDeUsuarioRegistrado";
                        var enviarEmailUsuarioRegistradoContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");
                        await httpClient.PostAsync(enviarEmailUsuarioRegistradoUrl, enviarEmailUsuarioRegistradoContent);

                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        var statusCode = (int)registrarUsuarioResponse.StatusCode;
                        var responseContent = await registrarUsuarioResponse.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, $"Ocurrió un error al registrar el usuario. Código de estado: {statusCode}, Respuesta: {responseContent}");
                        return View();
                        
                    }
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al procesar la solicitud");
                return View();
            }
        }


        // GET: UsuariosController/Create
        public ActionResult RestaurarContraseña()
        {

            return View();
        }

        // POST: UsuariosController/Create
        // POST: UsuariosController/RestaurarContraseña
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestaurarContraseña(Models.Usuario usuario, string confirmPassword)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Verificar existencia del usuario
                    var verificarUsuarioUrl = $"https://apicomercioapi.azure-api.net/api/Comercio/EncontrarUsuarioPorNombre?nombre={Uri.EscapeDataString(usuario.UserName)}";
                    var verificarUsuarioResponse = await httpClient.GetAsync(verificarUsuarioUrl);
                    if (!verificarUsuarioResponse.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError("", "Usuario no encontrado, por favor verifique.");
                        return View();
                    }

                 
                    var usuarioExistenteJson = await verificarUsuarioResponse.Content.ReadAsStringAsync();
                    var usuarioExistente = JsonConvert.DeserializeObject<Models.Usuario>(usuarioExistenteJson);
                    if (usuarioExistente == null)
                    {
                        ModelState.AddModelError("", "Usuario No encontrado, por favor verifique.");
                        return View();
                    }
                    if (usuario.PasswordHash == confirmPassword)
                    {
                        if (usuarioExistente.PasswordHash != null)
                        {
                            // Enviar la contraseña a la API para la encriptación
                            var generarclaveUrl = "https://apicomercioapi.azure-api.net/api/Comercio/EncriptarContrasena";
                            var generarclaveContent = new StringContent(JsonConvert.SerializeObject(usuario.PasswordHash), Encoding.UTF8, "application/json");
                            var generarclaveResponse = await httpClient.PutAsync(generarclaveUrl, generarclaveContent);
                            var claveEncriptada = await generarclaveResponse.Content.ReadAsStringAsync();

                            if (!generarclaveResponse.IsSuccessStatusCode)
                            {
                                ModelState.AddModelError("", "Ocurrió un error al encriptar la contraseña.");
                                return View();
                            }
                          

                            usuarioExistente.PasswordHash = claveEncriptada;
                            usuarioExistente.PhoneNumber = "0";
                            usuarioExistente.SecurityStamp = "null";
                            usuarioExistente.NormalizedEmail = usuarioExistente.Email;
                            usuarioExistente.ConcurrencyStamp = "null";
                            usuarioExistente.NormalizedUserName = usuarioExistente.UserName;
                            // Actualizar el usuario
                            var actualizarUsuarioUrl = "https://apicomercioapi.azure-api.net/api/Comercio/ActualizarUsuario";
                            var actualizarUsuarioContent = new StringContent(JsonConvert.SerializeObject(usuarioExistente), Encoding.UTF8, "application/json");
                            var actualizarUsuarioResponse = await httpClient.PutAsync(actualizarUsuarioUrl, actualizarUsuarioContent);
                           

                            if (!actualizarUsuarioResponse.IsSuccessStatusCode)
                            {
                                var statusCode = (int)actualizarUsuarioResponse.StatusCode;
                                var responseContent = await actualizarUsuarioResponse.Content.ReadAsStringAsync();
                                ModelState.AddModelError(string.Empty, $"Ocurrió un error al registrar el usuario. Código de estado: {statusCode}, Respuesta: {responseContent}");
                                return View();
                            }

                            // Enviar email de restauración de contraseña
                            var enviarEmailUrl = "https://apicomercioapi.azure-api.net/api/Comercio/EnviarEmailDeRestauracionDeClave";
                            var enviarEmailContent = new StringContent(JsonConvert.SerializeObject(usuarioExistente), Encoding.UTF8, "application/json");
                            var enviarEmailResponse = await httpClient.PostAsync(enviarEmailUrl, enviarEmailContent);
                            if (!enviarEmailResponse.IsSuccessStatusCode)
                            {
                                ModelState.AddModelError("", "Ocurrió un error al enviar el correo electrónico de restauración de contraseña.");
                                return View();
                            }

                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("", "La contraseña actual es nula.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("PasswordHash", "La contraseña y la confirmación no coinciden.");
                    }
                }
            }
            catch
            {
                ModelState.AddModelError("", "Ocurrió un error al procesar la solicitud.");
            }

            return View();
        }




        // GET: UsuariosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UsuariosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        

        // GET: UsuariosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UsuariosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
            
        }
        public ActionResult CerrarSesion()
        {
            return RedirectToAction("Index", "Usuarios");

        }
    }
}
