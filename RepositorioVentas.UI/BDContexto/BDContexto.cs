using Microsoft.EntityFrameworkCore;

namespace RepositorioVentas.UI.BDContexto
{
    public class BDContexto: DbContext
    {
        public BDContexto(DbContextOptions<BDContexto> opciones) : base(opciones)
        {
        }

        public DbSet<RepositorioVentas.UI.Models.Usuario> AspNetUsers { get; set; }
        public DbSet<RepositorioVentas.UI.Models.Inventario> Inventarios { get; set; }
        public DbSet<RepositorioVentas.UI.Models.AjusteDeInventario> AjusteDeInventarios { get; set; }
        public DbSet<RepositorioVentas.UI.Models.AperturaDeCaja> AperturasDeCaja { get; set; }
        public DbSet<RepositorioVentas.UI.Models.Ventas> Ventas { get; set; }
        public DbSet<RepositorioVentas.UI.Models.VentaDetalles> VentaDetalles { get; set; }

    }
}