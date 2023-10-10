using System.ComponentModel.DataAnnotations;

namespace Cyber_zad_1_.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Podaj aktualne hasło.")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Podaj nowe hasło.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
