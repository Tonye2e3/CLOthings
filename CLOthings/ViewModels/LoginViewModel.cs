using System.ComponentModel.DataAnnotations;

namespace CLOthings.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "帳號必填")]
        public string Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "密碼必填")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
