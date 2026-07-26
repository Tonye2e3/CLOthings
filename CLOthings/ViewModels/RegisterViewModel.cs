using System.ComponentModel.DataAnnotations;

namespace CLOthings.ViewModels
{

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "使用者名稱必填")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "帳號必填")]
        [StringLength(20, MinimumLength = 5)]
        public string Account { get; set; }

        [Required(ErrorMessage = "密碼必填")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "確認密碼必填")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "兩次密碼不一致")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "電子郵件必填")]
        [EmailAddress(ErrorMessage = "電子郵件格式錯誤")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "電話格式錯誤")]
        public string Phone { get; set; }
    }

}
