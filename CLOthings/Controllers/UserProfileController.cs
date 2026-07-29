
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
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create()
    {
        // 找出所有已經有 UserProfile 的使用者 ID
        var existingProfileUserIds = await _context.UserProfiles
            .Select(p => p.UserId)
            .ToListAsync();

        // 篩選出尚未建立個人資料的使用者
        var availableUsers = await _context.Users
            .Where(u => !existingProfileUserIds.Contains(u.UserId))
            .ToListAsync();

        // 管理員可以選擇任何使用者
        if (!availableUsers.Any())
        {
            TempData["Message"] = "所有使用者都已建立個人資料。";
            return RedirectToAction(nameof(Index));
        }

        ViewData["UserId"] = new SelectList(availableUsers, "UserId", "Account");

        return View();
    }



    // POST: USERPROFILES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserProfileCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var existingProfileUserIds = await _context.UserProfiles.Select(p => p.UserId).ToListAsync();

            var availableUsers = await _context.Users.Where(u => !existingProfileUserIds.Contains(u.UserId)).ToListAsync();
            ViewData["UserId"] = new SelectList(availableUsers, "UserId", "Account");

            return View(vm);
        }
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

        // 🔹 取得目前登入者的帳號
        var account = User.Identity?.Name;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Account == account);
        if (user == null) return NotFound();

        var entity = new UserProfile
        {
            UserId = User.IsInRole("SuperAdmin") ? vm.UserId : user.UserId,
            FirstName = vm.FirstName, // 你的名字
            LastName = vm.LastName, // 你的姓氏
            Gender = vm.Gender.ToString(),
            Birthday = vm.Birthday,
            Avatar = fileName != null ? $"/uploads/avatars/{fileName}" : null // 只存路徑
        };

        _context.Add(entity);
        await _context.SaveChangesAsync();

        // 🔹 SuperAdmin 回到列表，一般使用者回到個人資料
        return User.IsInRole("SuperAdmin")
    ? RedirectToAction(nameof(Index))
    : RedirectToAction(nameof(Profile));
    }



    // GET: USERPROFILES/Edit/5
    [Authorize(Roles = "SuperAdmin")]
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
    public async Task<IActionResult> Edit(int userprofileid, UserProfileCreateViewModel model)
    {
        var profileInDb = await _context.UserProfiles.FindAsync(userprofileid);
        if (profileInDb == null) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(model);
        }


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
        profileInDb.Gender = model.Gender.ToString();
        profileInDb.Birthday = model.Birthday;

        await _context.SaveChangesAsync();

        if (User.IsInRole("SuperAdmin"))
        {
            return RedirectToAction(nameof(Index));
        }
        else
        {
            return RedirectToAction(nameof(Details), new { userprofileid = profileInDb.UserProfileId });
        }
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

        if (profile == null)
        {
            // 一般使用者 → 自動建立一筆空白資料
            profile = new UserProfile
            {
                UserId = user.UserId,
                FirstName = "",
                LastName = "",
                Gender = "",
                Birthday = null,
                Avatar = null
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        return View("Edit", profile);
    }

}
