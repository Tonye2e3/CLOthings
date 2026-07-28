using System.ComponentModel.DataAnnotations;

namespace CLOthings.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "帳號必填")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "介於5~20個英數字元")]
        public string Account { get; set; } = string.Empty;


        [Required(ErrorMessage = "密碼必填")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "密碼至少 5  碼")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z]).+$",
        ErrorMessage = "密碼需包含至少一個大寫字母、一個小寫字母")]
        public string Password { get; set; } = string.Empty;
    }
}
