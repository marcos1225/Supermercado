using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositorioVentas.Model
{
    public class VentaDetalles
    {


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Propiedad de clave primaria generada por la base de datos

        ///
        public int Id_Venta { get; set; }
        public int Id_Inventario { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Monto { get; set; }
        public decimal MontoDescuento { get; set; }
    }
}
