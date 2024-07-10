using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositorioVentas.UI.Models
{
    public class Inventario
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El Nombre es requerido.")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "La Categoria es requerida.")]
        public int Categoria { get; set; }
        public int Cantidad { get; set; }
        [Required(ErrorMessage = "El Precio es requerido.")]
        public decimal Precio { get; set; }
    }

}
