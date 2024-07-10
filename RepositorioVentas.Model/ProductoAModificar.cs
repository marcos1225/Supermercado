using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositorioVentas.Model
{
    public class ProductoAModificar
    {
        [Required(ErrorMessage = "El campo nombre es requerido")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo categoria es requerido")]
        public int Categoria { get; set; }

        [Required(ErrorMessage = "El campo precio es requerido")]
        public decimal Precio { get; set; }
    }
}
