
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositorioVentas.Model
{
    public class Ventas
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Propiedad de clave primaria generada por la base de datos

        //Para usar en la venta
        public string NombreCliente { get; set; }
        public DateTime? Fecha { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal MontoDescuento { get; set; }
        public int TipoDePago { get; set; }
        public int PorcentajeDesCuento { get; set; }
        public string UserId { get; set; }
        public int Estado { get; set; }
        public int IdAperturaDeCaja { get; set; }
    }
}

