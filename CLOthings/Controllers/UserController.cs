
using CLOthings.Enums;
using CLOthings.Models;
using CLOthings.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


//[Route("/User/{action=index}/{userid?}")] //Attribute Routing 更換路由設定
public class UserController : Controller
{
    private readonly CLOthingsContext _context;

    public UserController(CLOthingsContext context)
    {
        _context = context;
    }

    // GET: USERS
    public async Task<IActionResult> Index()
    {
        return View(await _context.Users.ToListAsync());
    }

    // GET: USERS/Details/5
    public async Task<IActionResult> Details(int? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(m => m.UserId == userid);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // GET: USERS/Create
    public IActionResult Create()
    {
        var vm = new UserCreateViewModel
        {
            UserType = UserTypeEnum.User, // 一般使用者
            Status = StatusEnum.Active      // 啟用
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {

            return View(vm);
        }

        // 檢查帳號是否已存在
        if (_context.Users.Any(u => u.Account == vm.Account))
        {
            ModelState.AddModelError("Account", "此帳號已被註冊");
            return View(vm);
        }
        if (_context.Users.Any(u => u.Username == vm.Username))
        {
            ModelState.AddModelError("Username", "此使用者名稱已被使用");
            return View(vm);
        }

        // 將 ViewModel 轉換成 User 實體
        var user = new User
        {
            Username = vm.Username,
            Account = vm.Account,
            Password = vm.Password, // 建議加密
            Email = vm.Email,
            Phone = vm.Phone,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            UserType = vm.UserType,
            Status = vm.Status
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    // GET: USERS/Edit/5
    public async Task<IActionResult> Edit(int? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(userid);
        if (user == null)
        {
            return NotFound();
        }
        var vm = new UserCreateViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Account = user.Account,
            Password = user.Password,
            Email = user.Email,
            Phone = user.Phone,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            UserType = user.UserType,
            Status = user.Status,
            TwoFactorEnabled = user.TwoFactorEnabled
        };
        return View(vm);
    }

    // POST: USERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserCreateViewModel vm)
    {

        if (!ModelState.IsValid)
        {    // 驗證失敗 → 回傳原本的 View
            return View(vm);
        }


        try
        {
            var existingUser = await _context.Users.FindAsync(vm.UserId);
            if (existingUser == null)
            {
                return NotFound(vm);
            }

            // 更新必要欄位
            existingUser.Username = vm.Username;
            existingUser.Account = vm.Account;
            existingUser.Password = vm.Password;
            existingUser.Email = vm.Email;
            existingUser.Phone = vm.Phone;
            existingUser.UserType = vm.UserType;
            existingUser.Status = vm.Status;
            existingUser.TwoFactorEnabled = vm.TwoFactorEnabled;

            // ✅ 自動更新時間
            existingUser.UpdatedAt = DateTime.Now;

            // ❌ 不要動 CreatedAt
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(vm.UserId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));


        return View(vm);
    }

    // GET: USERS/Delete/5
    public async Task<IActionResult> Delete(int? userid)
    {
        if (userid == null)
        {
            return NotFound();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(m => m.UserId == userid);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // POST: USERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? userid)
    {
        var user = await _context.Users.FindAsync(userid);
        if (user != null)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool UserExists(int? userid)
    {
        return _context.Users.Any(e => e.UserId == userid);
    }



    //燈入夜面
    // GET: User/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: User/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {

        var sw = System.Diagnostics.Stopwatch.StartNew();
        Console.WriteLine($"登入開始時間：{DateTime.Now}");

        // 查詢使用者
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Account == vm.Account);
        Console.WriteLine($"查詢完成，用時：{sw.ElapsedMilliseconds} ms");

        if (user == null || user.Password != vm.Password)
        {
            ModelState.AddModelError("", "帳號或密碼錯誤");
            return View(vm);
        }

        if (user.UserType == UserTypeEnum.Banned)
        {
            ModelState.AddModelError("", "您的帳號已被封禁");
            return View(vm);
        }

        // 建立 Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Account),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.UserType.ToString()),
            new Claim("Username", user.Username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // 登入後依角色導向不同頁面
        switch (user.UserType)
        {
            case UserTypeEnum.SuperAdmin:
                return RedirectToAction("Dashboard", "Admin");
            case UserTypeEnum.Admin:
                return RedirectToAction("ManageProducts", "Admin");
            case UserTypeEnum.User:
                return RedirectToAction("Index", "Home");
            default:
                Console.WriteLine(DateTime.Now);
                return RedirectToAction("Login");
        }
    }

    // 登出
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }





    //註冊頁面
    // GET: User/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: User/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            // 除錯：顯示所有驗證錯誤
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
            return View(vm);
        }

        // 檢查帳號是否已存在
        if (_context.Users.Any(u => u.Account == vm.Account))
        {
            ModelState.AddModelError("Account", "此帳號已被註冊");
            return View(vm);
        }
        if (_context.Users.Any(u => u.Username == vm.Username))
        {
            ModelState.AddModelError("Username", "此使用者名稱已被使用");
            return View(vm);
        }

        var user = new User
        {
            Username = vm.Username,
            Account = vm.Account,
            Password = vm.Password, // 建議加密
            Email = vm.Email,
            Phone = vm.Phone,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            UserType = UserTypeEnum.User,   // 預設一般使用者
            Status = StatusEnum.Active      // 預設啟用
        };

        try
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("註冊失敗：" + ex.Message);
            ModelState.AddModelError("", "系統錯誤，請稍後再試");
            return View(vm);
        }

        TempData["SuccessMessage"] = "✅ 註冊成功！請登入您的帳號。";
        return RedirectToAction("Login");
    }





}

