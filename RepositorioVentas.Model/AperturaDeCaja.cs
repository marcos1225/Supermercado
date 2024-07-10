using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositorioVentas.Model
{
    public class AperturaDeCaja
    {
        [Required(ErrorMessage = "El id es requerido.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El id de usuario es requerido.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "La fecha de apertura es requerida.")]
        public DateTime? FechaDeInicio { get; set; }

        public DateTime? FechaDeCierre { get; set; }

        public string Observaciones { get; set; }

        [Required(ErrorMessage = "El estado es requerido.")]
        public Estado Estado { get; set; }
    }
}
