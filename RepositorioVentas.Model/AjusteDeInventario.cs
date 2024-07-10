using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RepositorioVentas.Model
{
    public class AjusteDeInventario
    {
        public int Id { get; set; }

        [HiddenInput]
        public int Id_Inventario { get; set; }

        [Display(Name = "Cantidad Actual")]
        public int CantidadActual { get; set; }

        [Required(ErrorMessage = "Campo Requerido.")]
        public int Ajuste { get; set; }

        [Required(ErrorMessage = "Campo Requerido.")]
        public int Tipo { get; set; }

        [Required(ErrorMessage = "Campo Requerido.")]
        public string Observaciones { get; set; }

        [Display(Name = "Usuario")]
        public string UserId { get; set; }
        public DateTime Fecha { get; set; }
    }
}
