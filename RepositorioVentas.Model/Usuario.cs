
using System.ComponentModel.DataAnnotations;

namespace RepositorioVentas.Model
{
    public class Usuario
    {
        [Required(ErrorMessage = "El id de usuario es requerido.")]
        public  string Id { get; set; }

        

        public  string UserName { get; set; }

        public  string NormalizedUserName { get; set; }

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Por favor, ingresa un correo electrónico válido.")]
        public  string Email { get; set; }

        public  string NormalizedEmail { get; set; }

        
        public  bool EmailConfirmed { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [DataType(DataType.Password)]
        public  string PasswordHash { get; set; }

        public  string SecurityStamp { get; set; }
        public  string ConcurrencyStamp { get; set; }

   
        public  string PhoneNumber { get; set; }

       
        public  bool PhoneNumberConfirmed { get; set; }

        public  bool TwoFactorEnabled { get; set; }
        public  DateTimeOffset? LockoutEnd { get; set; }
        public  bool LockoutEnabled { get; set; }
        public  int AccessFailedCount { get; set; }
    }
}
