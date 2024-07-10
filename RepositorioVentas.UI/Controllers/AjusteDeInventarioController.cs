using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RepositorioVentas.UI.Models;
using System.Net.Http.Headers;
namespace RepositorioVentas.UI.Controllers
{
    public class AjusteDeInventarioController : Controller
    {
        private RepositorioVentas.UI.BDContexto.BDContexto Conexion;
        private RepositorioVentas.UI.Data.Repositorio RegistroDeInventario;

        public AjusteDeInventarioController(RepositorioVentas.UI.BDContexto.BDContexto conexion, IMemoryCache CacheAcumuladoCaja, IMemoryCache CacheUsuarioIniciado, IMemoryCache elCache)
        {
            Conexion = conexion;
            RegistroDeInventario = new RepositorioVentas.UI.Data.Repositorio(conexion, CacheUsuarioIniciado, CacheUsuarioIniciado, elCache);
        }

        // GET: AjusteDeInventarioController
        public async Task<IActionResult> Index(string nombre)
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
        public async Task<IActionResult> IndexAjustes(int id)
        {
            List<Models.AjusteDeInventario> laListaDeAjustesAProducto;
            using (var httpClient = new HttpClient())
            {
                var url = $"https://apicomercioapi.azure-api.net/api/Comercio/ObtengaLosAjustesPorInventario?Id={id}";
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    laListaDeAjustesAProducto = JsonConvert.DeserializeObject<List<Models.AjusteDeInventario>>(json);
                    return View(laListaDeAjustesAProducto);
                }
                else
                {

                    return BadRequest();
                }
            }
        }


        // GET: AjusteDeInventarioController/Details/5
        public async Task<IActionResult> Details(int Id)
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"https://apicomercioapi.azure-api.net/api/Comercio/ObtengaElAjustePorId?Id={Id}";
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var ajusteinventario = JsonConvert.DeserializeObject<Models.AjusteDeInventario>(json);
                    return View(ajusteinventario);
                }
                else
                {

                    return BadRequest();
                }
            }
        }

        // GET: AjusteDeInventarioController/Create
        public async Task<IActionResult> Create(int Id)
        {
            try
            {
                // Obtener el inventario correspondiente al ID seleccionado
                using (var httpClient = new HttpClient())
                {
                    var url = $"https://apicomercioapi.azure-api.net/api/Comercio/ObtengaElInventarioPorId?Id={Id}";
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var inventario = JsonConvert.DeserializeObject<Models.Inventario>(json);

                        if (inventario != null)
                        {
                            // Crear un nuevo objeto AjusteDeInventario con los valores del inventario
                            var ajusteInventario = new AjusteDeInventario
                            {
                                Id_Inventario = Id,
                                CantidadActual = inventario.Cantidad,
                                Fecha = DateTime.Now // Establecer la fecha y hora actual
                            };

                            var urlObtenerUsuario = "https://apicomercioapi.azure-api.net/api/Comercio/Obtengausuario";
                            var responseUsuario = await httpClient.GetAsync(urlObtenerUsuario);

                            if (responseUsuario.IsSuccessStatusCode)
                            {
                                var jsonUsuario = await responseUsuario.Content.ReadAsStringAsync();
                               
                                ajusteInventario.UserId = jsonUsuario;

                                return View(ajusteInventario);
                            }
                            else
                            {
                                // Error al obtener el usuario desde la API
                                ModelState.AddModelError(string.Empty, "Error al obtener el usuario desde la API");
                                return View();
                            }
                        }
                        else
                        {
                            // El inventario no existe
                            return NotFound();
                        }
                    }
                    else
                    {
                        // Error al obtener el inventario desde la API
                        ModelState.AddModelError(string.Empty, "Error al obtener el inventario desde la API");
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                // Error en la aplicación
                ModelState.AddModelError(string.Empty, $"Error en la aplicación: {ex.Message}");
                return View();
            }
        }



        // POST: AjusteDeInventarioController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.AjusteDeInventario ajusteDeInventario)
        {
            try
            {
                var httpClient = new HttpClient();

                string json = JsonConvert.SerializeObject(ajusteDeInventario);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                var byteContent = new ByteArrayContent(buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                await httpClient.PostAsync("https://apicomercioapi.azure-api.net/api/Comercio/RegistreAjusteInventario", byteContent);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        // GET: AjusteDeInventarioController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AjusteDeInventarioController/Edit/5
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

        // GET: AjusteDeInventarioController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AjusteDeInventarioController/Delete/5
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
    }
}