using System.ComponentModel.DataAnnotations;

namespace ACCOB.ViewModels
{
    public class CrearAsesorViewModel
    {
        [Required(ErrorMessage = "El DNI es requerido")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe contener solo números")]
        [Display(Name = "DNI")]
        public string Dni { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El celular es requerido")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El celular debe tener 9 dígitos")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "El celular debe contener solo números")]
        [Display(Name = "Celular")]
        public string Celular { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }
    }
}