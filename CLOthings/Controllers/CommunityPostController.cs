
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

        var communitypost = await _context.CommunityPosts.FindAsync(id);
        if (communitypost == null)
        {
            return NotFound();
        }
        return View(communitypost);
    }

    // POST: COMMUNITYPOSTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("CommunityPostId,UserId,Content,Status")] CommunityPost communitypost)
    {
        if (id != communitypost.CommunityPostId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // 注意：若不想覆蓋原有的 PostDate，可以只更新特定欄位，或先從 DB 撈出實體再更新
                _context.Update(communitypost);
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
