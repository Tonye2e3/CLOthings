
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CLOthings.Models;

public class CommunityPostController : Controller
{
    private readonly CLOthingsContext _context;

    public CommunityPostController(CLOthingsContext context)
    {
        _context = context;
    }

    // GET: COMMUNITYPOSTS
    // GET: CommunityPost
    public async Task<IActionResult> Index(int page = 1)
    {
        int pageSize = 5; // 每頁顯示 5 筆資料 (可自由改成 10)

        // 1. 建立 basic Query，包含你需要顯示的標籤商品
        // 2. 加上 AsNoTracking() 讓 EF Core 不用花資源追蹤物件變更，大幅提升載入速度
        var query = _context.CommunityPosts
            .Include(p => p.User)
            .Include(p => p.PostImages)
            .Include(p => p.PostLikes)
            .Include(p => p.PostComments)
            .Include(p => p.PostTaggedProducts!)
                .ThenInclude(tp => tp.Product)       
            .AsNoTracking()
            .OrderByDescending(p => p.PostDate); // 讓最新貼文排在最前面

        // 2. 計算總筆數與總頁數
        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // 確保頁數不超出範圍
        page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

        // 3. 使用 Skip 和 Take 只從資料庫抓取當前頁面的資料
        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 傳送分頁資訊給 View
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View(posts);
    }

    // GET: COMMUNITYPOSTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var communitypost = await _context.CommunityPosts
            .Include(c => c.User)
            .Include(c => c.PostImages)
            .Include(c => c.PostTaggedProducts!)
                .ThenInclude(tp => tp.Product)
                .Include(c => c.PostComments!) // 👈 加上這行載入留言
                .ThenInclude(comment => comment.User) // 👈 若留言有關聯 User 也可以順便載入留言者
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CommunityPostId == id);
        if (communitypost == null)
        {
            return NotFound();
        }

        return View(communitypost);
    }

    // GET: COMMUNITYPOSTS/Create
    public async Task<IActionResult> Create()
    {
        // 假設預設管理員 ID 為 17 (或你登入的 Admin ID)
        ViewBag.AdminUserId = 17;
        ViewBag.AdminUserName = "superAdmin (官方管理員)";

        // 準備商品選單
        ViewBag.Products = _context.Products.ToList();
        return View();
    }

    // POST: COMMUNITYPOSTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UserId,Content,Status")] CommunityPost communitypost, List<IFormFile> imageFiles, List<int> selectedProductIds)
    {
        // 1. 如果前端沒傳 UserId (例如 0)，預設給一個管理員 ID (請依你的資料庫實際 ID 調整，例如 17)
        if (communitypost.UserId == 0)
        {
            communitypost.UserId = 17;
        }

        // 後端自動寫入發文時間
        communitypost.PostDate = DateTime.Now;

        // 檢查 Content 是否為空白、null，或是只輸入空格/換行
        if (string.IsNullOrWhiteSpace(communitypost.Content))
        {
            ModelState.AddModelError("Content", "貼文內容不能為空白！");
        }


        // 清除 UserId 的驗證狀態（避免 ModelState 誤判）
        ModelState.Remove("User");

        if (ModelState.IsValid)
        {
            // 1. 先新增貼文主體
            _context.Add(communitypost);
            await _context.SaveChangesAsync();

            // 2. 處理圖片上傳 (PostImages)
            if (imageFiles != null && imageFiles.Count > 0)
            {
                foreach (var file in imageFiles)
                {
                    if (file.Length > 0)
                    {
                        // 產生不重複的檔名
                        var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);

                        // 2. 指定實體儲存路徑
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/posts", fileName);

                        // 3. 把圖片存進 wwwroot 資料夾
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // 4. 寫入資料庫路徑
                        var postImage = new PostImage
                        {
                            CommunityPostId = communitypost.CommunityPostId, 
                            ImageFileName = "/images/posts/" + fileName,
                            SortOrder = 1
                        };
                        _context.PostImages.Add(postImage);
                    }
                }
                
            }

            // 3. 處理標籤商品 (PostTaggedProducts)
            if (selectedProductIds != null && selectedProductIds.Count > 0)
            {
                foreach (var productId in selectedProductIds)
                {
                    var taggedProduct = new PostTaggedProduct
                    {
                        CommunityPostId = communitypost.CommunityPostId,
                        ProductId = productId
                    };
                    _context.PostTaggedProducts.Add(taggedProduct);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Users = await _context.Users.ToListAsync();
        ViewBag.Products = await _context.Products.ToListAsync();
        return View(communitypost);
    }

    // GET: COMMUNITYPOSTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var communityPost = await _context.CommunityPosts
        .Include(c => c.User)
        .Include(c => c.PostImages)
        .Include(c => c.PostTaggedProducts!)
            .ThenInclude(tp => tp.Product)
        .FirstOrDefaultAsync(m => m.CommunityPostId == id);

        if (communityPost == null)
        {
            return NotFound();
        }

        // 準備商品勾選清單（供管理者調整標籤商品）
        var selectedProductIds = communityPost.PostTaggedProducts?.Select(tp => tp.ProductId).ToList() ?? new List<int>();
        ViewBag.Products = await _context.Products
            .Select(p => new {
                p.ProductId,
                p.ProductName,
                IsSelected = selectedProductIds.Contains(p.ProductId)
            }).ToListAsync();
        return View(communityPost);
    }

    // POST: COMMUNITYPOSTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string Status, List<int> selectedProductIds)
    {
        // 從資料庫撈出該筆貼文及其標籤商品
        var postToUpdate = await _context.CommunityPosts
            .Include(c => c.PostTaggedProducts)
            .FirstOrDefaultAsync(p => p.CommunityPostId == id);

        if (postToUpdate == null)
        {
            return NotFound();
        }

        try
        {
            // 1. 僅更新管理者權限欄位（狀態切換：public / hidden）
            postToUpdate.Status = Status;

            // 2. 更新標籤商品（先清空舊標籤，再建立勾選的新標籤）
            if (postToUpdate.PostTaggedProducts != null)
            {
                _context.PostTaggedProducts.RemoveRange(postToUpdate.PostTaggedProducts);
            }

            if (selectedProductIds != null && selectedProductIds.Any())
            {
                foreach (var productId in selectedProductIds)
                {
                    _context.PostTaggedProducts.Add(new PostTaggedProduct
                    {
                        CommunityPostId = id,
                        ProductId = productId
                    });
                }
            }

            // 3. 儲存所有變更
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.CommunityPosts.Any(e => e.CommunityPostId == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
    }

    // GET: COMMUNITYPOSTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var communitypost = await _context.CommunityPosts.Include(c => c.User)
            .FirstOrDefaultAsync(m => m.CommunityPostId == id);
        if (communitypost == null)
        {
            return NotFound();
        }

        return View(communitypost);
    }

    // POST: COMMUNITYPOSTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var communitypost = await _context.CommunityPosts.FindAsync(id);
        if (communitypost != null)
        {

            // 1. 先清除標籤商品關聯資料
            var taggedProducts = _context.PostTaggedProducts.Where(p => p.CommunityPostId == id);
            _context.PostTaggedProducts.RemoveRange(taggedProducts);

            // 2. 清除相關圖片紀錄，並刪除 wwwroot 裡的實體圖檔
            var images = _context.PostImages.Where(img => img.CommunityPostId == id).ToList();
            foreach (var img in images)
            {
                if (!string.IsNullOrEmpty(img.ImageFileName))
                {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageFileName.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            _context.PostImages.RemoveRange(images);

            // 3. 清除相關留言（若專案有此資料表可取消註解）
            var comments = _context.PostComments.Where(c => c.CommunityPostId == id);
            _context.PostComments.RemoveRange(comments);

            // 4. 清除相關按讚（若專案有此資料表可取消註解）
            var likes = _context.PostLikes.Where(l => l.CommunityPostId == id);
            _context.PostLikes.RemoveRange(likes);


            // 5. 最後刪除貼文主體
            _context.CommunityPosts.Remove(communitypost);

            // 6. 寫入資料庫變更
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: CommunityPosts/DeleteComment/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int commentId, int postId)
    {
        var comment = await _context.PostComments.FindAsync(commentId);
        if (comment != null)
        {
            _context.PostComments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        // 刪除後重定向回原本貼文的 Details 頁面
        return RedirectToAction(nameof(Details), new { id = postId });
    }

    private bool CommunityPostExists(int? id)
    {
        return _context.CommunityPosts.Any(e => e.CommunityPostId == id);
    }
}
