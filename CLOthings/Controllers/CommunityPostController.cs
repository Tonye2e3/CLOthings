
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
    public async Task<IActionResult> Index()
    {
        // 使用 .Include() 將圖片與標籤商品關聯資料一起撈出來
        var posts = await _context.CommunityPosts 
            .Include(p => p.PostImages)          // 必須加上這行才能拿到圖片路徑！
            .Include(p => p.PostTaggedProducts)
                .ThenInclude(tp => tp.Product) //  順便把商品名稱載入進來
            .Include(p => p.PostLikes)          // 新增這行：載入按讚資料
            .Include(p => p.PostComments)       // 新增這行：載入留言資料
            .ToListAsync();

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
        // 撈出所有使用者清單與商品清單給前端選單使用
        ViewBag.Users = await _context.Users.ToListAsync();
        ViewBag.Products = await _context.Products.ToListAsync();
        return View();
    }

    // POST: COMMUNITYPOSTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UserId,Content,Status")] CommunityPost communitypost, List<IFormFile> imageFiles, List<int> selectedProductIds)
    {
        // 後端自動寫入發文時間
        communitypost.PostDate = DateTime.Now;

        // 不再寫死 UserId，讓它讀取前端選取的 UserId
        // 移除：ModelState.Remove("UserId");


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

        // 1. 關鍵！加上 .Include() 將圖片與標籤商品撈出來
        var communityPost = await _context.CommunityPosts
                .Include(p => p.PostImages)
                .Include(p => p.PostTaggedProducts)
                .FirstOrDefaultAsync(m => m.CommunityPostId == id); if (communityPost == null)
        {
            return NotFound();
        }

        // 2. 關鍵！撈出所有商品清單傳給 View，這樣標籤商品區才會有核取方塊 (Checkbox) 可以勾選
        ViewBag.ProductList = await _context.Products!.ToListAsync();
        return View(communityPost);
    }

    // POST: COMMUNITYPOSTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CommunityPostId,UserId,Content,Status,PostDate")] CommunityPost communitypost, int[]? selectedProductIds, List<IFormFile>? imageFiles,
    int[]? deleteImageIds)
    {
        if (id != communitypost.CommunityPostId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // 1. 解決日期溢位問題：若傳進來的 PostDate 為預設值 0001/01/01，則預設賦予目前時間
                if (communitypost.PostDate == default(DateTime))
                {
                    communitypost.PostDate = DateTime.Now;
                }

                // 2. 更新貼文主體基本資料
                _context.Update(communitypost);

                // 3. 更新「標籤商品」關聯資料
                // 先抓出該貼文現有的標籤商品
                var existingTags = _context.PostTaggedProducts.Where(p => p.CommunityPostId == id);
                _context.PostTaggedProducts.RemoveRange(existingTags); // 先清空舊標籤

                // 如果使用者有勾選新的商品，再重新建立關聯
                if (selectedProductIds != null && selectedProductIds.Length > 0)
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
                // --- 4. 處理刪除勾選的圖片 (新增這段) ---
                if (deleteImageIds != null && deleteImageIds.Length > 0)
                {
                    // 抓出要刪除的圖片紀錄
                    var imagesToDelete = await _context.PostImages
                        .Where(img => deleteImageIds.Contains(img.PostImageId))
                        .ToListAsync();

                    if (imagesToDelete.Any())
                    {
                        // 從資料庫刪除紀錄
                        _context.PostImages.RemoveRange(imagesToDelete);

                        // (可選) 順便刪除 wwwroot 裡的實體檔案，避免產生垃圾檔案：
                        foreach (var img in imagesToDelete)
                        {
                            if (!string.IsNullOrEmpty(img.ImageFileName))
                            {
                                string localFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageFileName.TrimStart('/'));
                                if (System.IO.File.Exists(localFilePath))
                                {
                                    System.IO.File.Delete(localFilePath);
                                }
                            }
                        }
                    }
                }
                // --- 5. 處理新上傳的圖片 ---
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    // 設定圖片儲存路徑 (wwwroot/images/posts)
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "posts");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            // 產生檔名（避免檔名重複）
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            // 儲存檔案到伺服器實體路徑
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            // 新增紀錄到 PostImage 資料表
                            _context.PostImages.Add(new PostImage
                            {
                                CommunityPostId = id,
                                ImageFileName = "/images/posts/" + uniqueFileName, // 對應你資料庫欄位 ImageFileName
                                SortOrder = 1
                            });
                        }
                    }
                }

                // 儲存所有變更
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommunityPostExists(communitypost.CommunityPostId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // 若驗證失敗重新回傳 View，記得把商品選單重新補上，畫面才不會壞掉
        ViewBag.ProductList = await _context.Products!.ToListAsync();
        return View(communitypost);
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
            _context.CommunityPosts.Remove(communitypost);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CommunityPostExists(int? id)
    {
        return _context.CommunityPosts.Any(e => e.CommunityPostId == id);
    }
}
