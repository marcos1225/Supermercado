using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Net.Http.Headers;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RepositorioVentas.UI.Controllers
{
    public class InventarioController : Controller
    {
        private RepositorioVentas.UI.BDContexto.BDContexto Conexion;
        private RepositorioVentas.UI.Data.Repositorio RegistroDeInventario;

        public InventarioController(RepositorioVentas.UI.BDContexto.BDContexto conexion, IMemoryCache CacheAcumuladoCaja, IMemoryCache CacheUsuarioIniciado, IMemoryCache elCache)
        {
            Conexion = conexion;
            RegistroDeInventario = new RepositorioVentas.UI.Data.Repositorio(conexion, CacheUsuarioIniciado, CacheUsuarioIniciado, elCache);
        }
        // GET: InventarioController

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

        // GET: InventarioController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"https://apicomercioapi.azure-api.net/api/Comercio/ObtengaElInventarioPorId?Id={id}";
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var inventario = JsonConvert.DeserializeObject<Models.Inventario>(json);
                    return View(inventario);
                }
                else
                {

                    return BadRequest();
                }
            }
        }

        // GET: InventarioController/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.Inventario inventario)
        {
            try
            {
                var httpClient = new HttpClient();

                string json = JsonConvert.SerializeObject(inventario);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                var byteContent = new ByteArrayContent(buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                await httpClient.PostAsync("https://apicomercioapi.azure-api.net/api/Comercio/RegistreInventario", byteContent);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: InventarioController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"https://apicomercioapi.azure-api.net/api/Comercio/ObtengaElInventarioPorId?Id={id}";
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var inventario = JsonConvert.DeserializeObject<Models.Inventario>(json);
                    return View(inventario);
                }
                else
                {

                    return BadRequest();
                }
            }
        }
        // POST: InventarioController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Models.Inventario inventario)
        {
            try
            {
                var httpClient = new HttpClient();

                string json = JsonConvert.SerializeObject(inventario);

                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                var byteContent = new ByteArrayContent(buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await httpClient.PostAsync("https://apicomercioapi.azure-api.net/api/Comercio/Edite", byteContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, "Ocurrió un error: " + errorResponse);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error en la aplicación: {ex.Message}");
            }

            return View();
        }



        // GET: InventarioController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: InventarioController/Delete/5
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
