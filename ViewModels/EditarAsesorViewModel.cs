using System.ComponentModel.DataAnnotations;

namespace ACCOB.ViewModels
{
    public class EditarAsesorViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre es demasiado largo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(@"^\d{8,12}$", ErrorMessage = "El DNI debe tener entre 8 y 12 dígitos")]
        public string Dni { get; set; }

        [Phone(ErrorMessage = "Formato de celular no válido")]
        public string? Celular { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
        public string? NewPassword { get; set; }
    }
}