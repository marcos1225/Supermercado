using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositorioVentas.Model
{
    public class AcumuladoCaja
    {
        public AcumuladoCaja()
        {

            Efectivo = 0;
            Tarjeta = 0;
            SINPEMovil = 0;
            Activo = false;
            FechaDeInicio = null;
        }
        public DateTime? FechaDeInicio { get; set; }
        public decimal Efectivo { get; set; }
        public decimal Tarjeta { get; set; }
        public decimal SINPEMovil { get; set; }
        public Boolean Activo { get; set; }
    }
}
