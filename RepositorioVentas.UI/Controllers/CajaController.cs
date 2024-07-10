using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RepositorioVentas.UI.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;


namespace RepositorioVentas.UI.Controllers
{

    public class CajaController : Controller
    {
        private RepositorioVentas.UI.BDContexto.BDContexto Conexion;
        public RepositorioVentas.UI.Data.Repositorio RepositorioDeCaja;
        Models.AcumuladoCaja acumuladoCaja = new Models.AcumuladoCaja();  
        // GET: CajaController
        public CajaController(BDContexto.BDContexto conexion, IMemoryCache CacheAcumuladoCaja, IMemoryCache CacheUsuarioIniciado, IMemoryCache elCache)
        {

            Conexion = conexion;
            RepositorioDeCaja = new Data.Repositorio(conexion, CacheAcumuladoCaja, CacheUsuarioIniciado, elCache);

        }
        public async Task<ActionResult> Index()
        {
            AcumuladoCaja Acumulado = new AcumuladoCaja();

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    string apiUrl = "https://apicomercioapi.azure-api.net/api/Comercio/ObtenerAcumulado";
                    var response = await httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        var acumulado = JsonConvert.DeserializeObject<Models.AcumuladoCaja>(jsonResponse);

                        if (acumulado != null)
                        {
                            Acumulado.Efectivo = acumulado.Efectivo;
                            Acumulado.Tarjeta = acumulado.Tarjeta;
                            Acumulado.SINPEMovil = acumulado.SINPEMovil;
                            Acumulado.FechaDeInicio = acumulado.FechaDeInicio;
                            Acumulado.Activo = acumulado.Activo;
                        }
                        return View(Acumulado);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error al obtener los datos de acumulado desde la API");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error en la aplicación: {ex.Message}");
            }

            return View(Acumulado);
        }


        // GET: CajaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }
        public async Task<ActionResult> AbrirCaja()
        {
            try
            {
                Console.WriteLine("Caja Abierta");

                using (var httpClient = new HttpClient())
                {
                    var url = "https://apicomercioapi.azure-api.net/api/Comercio/RegistreAperturaCaja";
                    var response = await httpClient.PostAsync(url, null);

                    if (response.IsSuccessStatusCode)
                    {
                        //var jsonResponse = await response.Content.ReadAsStringAsync();
                        var Respuesta = JsonConvert.DeserializeObject<bool>(await response.Content.ReadAsStringAsync());
                        if (Respuesta == true)
                        {
                            return View();
                        }
                        else
                        {
                            return RedirectToAction("Index");
                        }

                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, "Ocurrió un error : " + errorResponse);
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error en la aplicación: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
        public async Task<ActionResult> CerrarCaja()
        {
            try
            {
                Console.WriteLine("Caja cerrada");

                using (var httpClient = new HttpClient())
                {
                    var url = "https://apicomercioapi.azure-api.net/api/Comercio/CierreAperturaCaja";
                    var response = await httpClient.PostAsync(url, null);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var acumulado = JsonConvert.DeserializeObject<Models.AcumuladoCaja>(jsonResponse);
                        return View(acumulado);
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, "Ocurrió un error : " + errorResponse);
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error en la aplicación: {ex.Message}");
                return RedirectToAction("Index");
            }
        }



        // Para Realizar la Venta
        public async Task<IActionResult> IndexVenta(string nombre)
        {
            List<Models.Inventario> laListaDeProductos;
            var httpClient = new HttpClient();

            if (nombre is null)
            {
                var respuesta = await httpClient.GetAsync("https://apicomercioapi.azure-api.net/api/Comercio/ObtengaElInventario");
                string apiResponse = await respuesta.Content.ReadAsStringAsync();
                laListaDeProductos = JsonConvert.DeserializeObject<List<Models.Inventario>>(apiResponse);
                return View(laListaDeProductos);
            }
            else
            {

                var query = new Dictionary<string, string>()
                {

                    ["nombre"] = nombre
                };

                var uri = QueryHelpers.AddQueryString("https://apicomercioapi.azure-api.net/api/Comercio/ObtengaLaListaPorNombre", query);

                var response = await httpClient.GetAsync(uri);
                string apiResponse = await response.Content.ReadAsStringAsync();
                laListaDeProductos = JsonConvert.DeserializeObject<List<Models.Inventario>>(apiResponse);

                return View(laListaDeProductos);

            }


        }

        // POST: VentasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVenta(Models.Ventas ventas)
        {
            try
            {
                var httpClient = new HttpClient();

                string json = JsonConvert.SerializeObject(ventas);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                var byteContent = new ByteArrayContent(buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
              var response =  await httpClient.PostAsync("https://apicomercioapi.azure-api.net/api/Comercio/RegistreVenta", byteContent);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, "Ocurrió un error : " + errorResponse);
                    Console.WriteLine(errorResponse);
                    return View();
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> RegistroInventarioVentas(Models.VentasInventario ventasinventario)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Crear el objeto de venta de inventario
                   

                    // Realizar la solicitud POST a la API de registro de venta de inventario
                    var url = "https://apicomercioapi.azure-api.net/api/Comercio/RegistreInventarioVenta";
                    var ventaInventarioJson = JsonConvert.SerializeObject(ventasinventario);
                    var content = new StringContent(ventaInventarioJson, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(url, content);

                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al registrar la venta: " + ex.Message);
                return Content("");
            }
        }


       [HttpPost]
          public async Task<ActionResult> EliminarInventarioVentas(int Identificador_Ventas)
{
               try
               {
                  using (var httpClient = new HttpClient())
                   {
                  // Realizar la solicitud POST a la API para eliminar el inventario de ventas
                 var url = $"https://apicomercioapi.azure-api.net/api/Comercio/EliminarInventarioVenta?Identificador_Ventas={Identificador_Ventas}";
                 var response = await httpClient.PostAsync(url, null);
 
               if (response.IsSuccessStatusCode)
                {
                  return Content("  " + Identificador_Ventas);
                 }
               else
              {
                ModelState.AddModelError(string.Empty, "Error al eliminar el inventario de ventas");
                return Content("");
              }
           }
      }
         catch (Exception ex)
         {
              ModelState.AddModelError(string.Empty, "Error al eliminar el inventario de ventas: " + ex.Message);
              return Content("");
         }
}

        public async Task<ActionResult> ObtenerUserId()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Realizar la solicitud GET a la API para obtener el UserId
                    var url = "https://apicomercioapi.azure-api.net/api/Comercio/Obtengausuario";
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var userId = await response.Content.ReadAsStringAsync();

                        return Json(userId);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error al obtener el UserId");
                        return Json("");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al obtener el UserId: " + ex.Message);
                return Json("");
            }
        }


        public async Task<ActionResult> ObtenerIdCaja()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Realizar la solicitud GET a la API para obtener el IdCaja
                    var url = "https://apicomercioapi.azure-api.net/api/Comercio/ObtengaIdCaja";
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var idCaja = await response.Content.ReadAsStringAsync();

                        return Json(idCaja);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error al obtener el IdCaja");
                        return Json("");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al obtener el IdCaja: " + ex.Message);
                return Json("");
            }
        }


    }
}
