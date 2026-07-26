using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class AdminController : Controller
{
    // 最高管理員專屬 Dashboard
    [Authorize(Roles = "SuperAdmin")] // 確保已登入
    public IActionResult Dashboard()
    {
        // 這裡可以放最高管理員專屬功能，例如管理所有使用者
        return View();
    }

    // 一般管理員專屬商品管理
    [Authorize(Roles = "Admin")]
    public IActionResult ManageProducts()
    {
        // 這裡可以放一般管理員專屬功能，例如商品 CRUD
        return View();
    }
}
