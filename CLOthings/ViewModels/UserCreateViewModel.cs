using CLOthings.Enums;
using System.ComponentModel.DataAnnotations;

namespace CLOthings.ViewModels
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "使用者名稱必填")]
        [StringLength(50, ErrorMessage = "使用者名稱不可超過 50 字")]
        public string Username { get; set; }

        [Required(ErrorMessage = "帳號必填")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "帳號長度需介於 5~20 字")]
        public string Account { get; set; }

        [Required(ErrorMessage = "密碼必填")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼請大於6位數")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "電子郵件必填")]

        [EmailAddress(ErrorMessage = "請輸入正確的電子郵件格式")]
        public string Email { get; set; }

        [Required(ErrorMessage = "電話號碼必填")]
        [Phone(ErrorMessage = "請輸入正確的電話號碼")]
        public string Phone { get; set; }

        public UserTypeEnum UserType { get; set; }
        public StatusEnum Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
