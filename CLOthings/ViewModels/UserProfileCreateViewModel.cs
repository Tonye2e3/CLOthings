using CLOthings.Enums;
using CLOthings.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLOthings.ViewModels
{
    public class UserProfileCreateViewModel
    {

        public int UserProfileId { get; set; }


        [Display(Name = "使用者帳號")]
        public int UserId { get; set; }

        [Display(Name = "名字")]
        public string? FirstName { get; set; }

        [Display(Name = "姓氏")]
        public string? LastName { get; set; }

        // 這個是存在資料庫的路徑，保留
        public string? Avatar { get; set; }

        [Display(Name = "性別")]
        public GenderEnum Gender { get; set; }

        [Display(Name = "生日")]
        public DateOnly? Birthday { get; set; }

        // 這個是不存DB，專門接上傳檔案用的
        [Display(Name = "頭像")]
        [NotMapped]
        public IFormFile? AvatarFile { get; set; }
    }
}
