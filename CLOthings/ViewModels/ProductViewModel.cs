using System.ComponentModel.DataAnnotations;

namespace CLOthings.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage ="此欄位必填")]
        [Display(Name = "商品名稱")]
        public string ProductName { get; set; }

        [Required(ErrorMessage ="此欄位必填")]
        [Display(Name = "供應商")]
        public int? SupplierId { get; set; }

        [Required(ErrorMessage = "此欄位必填")]
        [Display(Name = "商品分類")]
        public int? ProductCategoryId { get; set; }

        //[Display(Name = "商品分類")]
        //public string ProductCategoryName { get; set; }

        [Display(Name = "商品描述")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "此欄位必填")]
        [Display(Name = "價格")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "此欄位必填")]
        [Display(Name = "上架狀態")]
        public string Status { get; set; }

        [Display(Name = "折扣開始日期")]
        public DateTime? SalesStartDate { get; set; }

        [Display(Name = "折扣結束日期")]
        public DateTime? SalesEndDate { get; set; }

        [Display(Name = "商品圖片")]
        public List<IFormFile>? ProductImg { get; set; }

        [Display(Name = "商品規格")]
        public List<ProductSpecificationViewModel>? ProductSpecifications { get; set; }


        public List<ExistingProductImgViewModel>? ExistingImages { get; set; }  // 目前資料庫已有的圖片

        public List<int>? ImagesToDelete { get; set; }  // 使用者勾選要刪除的圖片ID
    }

    public class ProductSpecificationViewModel
    {
        //[Required(ErrorMessage = "此欄位必填")]
        [Display(Name = "尺寸")]
        public string? Size { get; set; }

        [Required(ErrorMessage = "此欄位必填")]
        [Display(Name = "顏色")]
        public string? Color { get; set; }

        [Required(ErrorMessage = "此欄位必填")]
        [Display(Name = "庫存數量")]
        public int? Inventory { get; set; }

        [Display(Name = "在途庫存")]
        public int? UnitOnOrder { get; set; }

        [Required(ErrorMessage = "此欄位必填")]
        [Display(Name = "再訂量")]
        public int? ReorderLevel { get; set; }

    }

    public class ExistingProductImgViewModel
    {
        public int ProductImgId { get; set; }
        public string ProductImgFile { get; set; }
    }
}
