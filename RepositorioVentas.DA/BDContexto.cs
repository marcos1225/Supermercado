using Microsoft.EntityFrameworkCore;

namespace RepositorioVentas.DA
{
    public class BDContexto: DbContext
    {
        public BDContexto(DbContextOptions<BDContexto> opciones) : base(opciones)
        {
        }

        public DbSet<RepositorioVentas.Model.Usuario> AspNetUsers { get; set; }
        public DbSet<RepositorioVentas.Model.Inventario> Inventarios { get; set; }
        public DbSet<RepositorioVentas.Model.AjusteDeInventario> AjusteDeInventarios { get; set; }
        public DbSet<RepositorioVentas.Model.AperturaDeCaja> AperturasDeCaja { get; set; }
        public DbSet<RepositorioVentas.Model.Ventas> Ventas { get; set; }
        public DbSet<RepositorioVentas.Model.VentaDetalles> VentaDetalles { get; set; }

    }
}