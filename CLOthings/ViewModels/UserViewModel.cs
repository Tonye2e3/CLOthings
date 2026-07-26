using CLOthings.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CLOthings.ViewModels
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public UserTypeEnum UserType { get; set; }

        // 下拉選單用
        public SelectList UserTypeOptions { get; set; }
    }

}
