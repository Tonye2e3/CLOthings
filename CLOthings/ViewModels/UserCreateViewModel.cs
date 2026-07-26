using CLOthings.Enums;
using System.ComponentModel.DataAnnotations;

namespace CLOthings.ViewModels
{
    public class UserCreateViewModel : LoginViewModel
    {
        [Required(ErrorMessage = "使用者名稱必填")]
        [StringLength(50, ErrorMessage = "使用者名稱不可超過 50 字")]
        public string Username { get; set; }

        [Required(ErrorMessage = "電子郵件必填")]

        [EmailAddress(ErrorMessage = "請輸入正確的電子郵件格式")]
        public string Email { get; set; }

        [Required(ErrorMessage = "電話號碼必填")]
        [Phone(ErrorMessage = "請輸入正確的電話號碼")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "請選擇使用者類型")]
        public UserTypeEnum UserType { get; set; }

        [Required(ErrorMessage = "請選擇帳戶狀態")]
        public StatusEnum Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
