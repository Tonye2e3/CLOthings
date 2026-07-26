
using CLOthings.Models;
using CLOthings.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class UserProfileController : Controller
{
    private readonly CLOthingsContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment; // 1. 加這個

    public UserProfileController(CLOthingsContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;

    }

    // GET: USERPROFILES
    public async Task<IActionResult> Index()
    {
        var profiles = await _context.UserProfiles
        .Include(p => p.User) // ✅ 載入關聯的 User
        .ToListAsync();

        return View(profiles);
    }

    // GET: USERPROFILES/Details/5
    public async Task<IActionResult> Details(int? userprofileid)
    {
        if (userprofileid == null)
        {
            return NotFound();
        }

        var userprofile = await _context.UserProfiles
            .FirstOrDefaultAsync(m => m.UserProfileId == userprofileid);
        if (userprofile == null)
        {
            return NotFound();
        }

        return View(userprofile);
    }

    // GET: USERPROFILES/Create
    [Authorize]
    public async Task<IActionResult> Create() // 1. 一定要是 async Task
    {
        var account = User.Identity?.Name;
        if (string.IsNullOrEmpty(account)) return RedirectToAction("Login", "User");

        // 2. 有 Async 就要有 await
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Account == account);
        if (user == null) return NotFound();

        // 3. AnyAsync 也要 await
        if (await _context.UserProfiles.AnyAsync(p => p.UserId == user.UserId))
        {
            return RedirectToAction("Profile");
        }

        ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Account", user.UserId);

        var vm = new UserProfileCreateViewModel
        {
            UserId = user.UserId
        };
        return View(vm);
    }



    // POST: USERPROFILES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserProfileCreateViewModel vm)
    {
        if (ModelState.IsValid)
        {
            string? fileName = null;

            // 有上傳檔案才處理
            if (vm.AvatarFile != null && vm.AvatarFile.Length > 0)
            {
                // 1. 確保資料夾存在
                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadFolder);

                // 2. 用 GUID 重新命名，避免檔名重複
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.AvatarFile.FileName);
                var filePath = Path.Combine(uploadFolder, fileName);

                // 3. 存檔
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.AvatarFile.CopyToAsync(stream);
                }

            }

            var entity = new UserProfile
            {
                UserId = vm.UserId,
                FirstName = vm.FirstName, // 你的名字
                LastName = vm.LastName, // 你的姓氏
                Gender = vm.Gender.ToString(),
                Birthday = vm.Birthday,
                Avatar = fileName != null ? $"/uploads/avatars/{fileName}" : null // 只存路徑
            };

            _context.Add(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Profile));
        }
        ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Account", vm.UserId);
        return View(vm);
    }

    // GET: USERPROFILES/Edit/5
    public async Task<IActionResult> Edit(int? userprofileid)
    {
        if (userprofileid == null)
        {
            return NotFound();
        }

        var userprofile = await _context.UserProfiles.FindAsync(userprofileid);
        if (userprofile == null)
        {
            return NotFound();
        }
        return View(userprofile);
    }

    // POST: USERPROFILES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int userprofileid, UserProfile model)
    {
        var profileInDb = await _context.UserProfiles.FindAsync(userprofileid);
        if (profileInDb == null) return NotFound();

        if (ModelState.IsValid)
        {
            // 有上傳新圖才換
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.AvatarFile.FileName);
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                // 刪舊圖
                if (!string.IsNullOrEmpty(profileInDb.Avatar))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, profileInDb.Avatar.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                profileInDb.Avatar = $"/uploads/avatars/{fileName}";
            }

            profileInDb.FirstName = model.FirstName;
            profileInDb.LastName = model.LastName;
            profileInDb.Gender = model.Gender;
            profileInDb.Birthday = model.Birthday;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Profile));
        }
        return View(model);
    }

    // GET: USERPROFILES/Delete/5
    public async Task<IActionResult> Delete(int? userprofileid)
    {
        if (userprofileid == null)
        {
            return NotFound();
        }

        var userprofile = await _context.UserProfiles
            .FirstOrDefaultAsync(m => m.UserProfileId == userprofileid);
        if (userprofile == null)
        {
            return NotFound();
        }

        return View(userprofile);
    }

    // POST: USERPROFILES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? userprofileid)
    {
        var userprofile = await _context.UserProfiles.FindAsync(userprofileid);
        if (userprofile != null)
        {
            _context.UserProfiles.Remove(userprofile);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool UserProfileExists(int? userprofileid)
    {
        return _context.UserProfiles.Any(e => e.UserProfileId == userprofileid);
    }

    //個人資料檢視
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var account = User.Identity?.Name;
        if (string.IsNullOrEmpty(account)) return RedirectToAction("Login", "User");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Account == account);
        if (user == null) return NotFound($"找不到帳號 {account}");


        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.UserId);

        // ✅ 如果使用者還沒有建立個人資料，導向建立頁面
        if (profile == null)
        {
            return RedirectToAction("Create");
        }
        return View("Details", profile);

    }
}
