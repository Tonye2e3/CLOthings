using System.ComponentModel.DataAnnotations;

namespace CLOthings.ViewModels
{

    public class RegisterViewModel : LoginViewModel
    {
        [Required(ErrorMessage = "使用者名稱必填")]
        [StringLength(50, ErrorMessage = "使用者名稱不可超過 50 字")]
        public string Username { get; set; }

        [Required(ErrorMessage = "確認密碼必填")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "兩次密碼不一致")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "電子郵件必填")]
        [EmailAddress(ErrorMessage = "電子郵件格式錯誤")]
        public string Email { get; set; }

        [Required(ErrorMessage = "必田")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "請輸入正確的手機號碼")]
        public string Phone { get; set; }

        [Display(Name = "創建時間")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }
    }

}
