
using CLOthings.Models;
using CLOthings.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

public class ProductController : Controller
{
    private readonly CLOthingsContext _context;
    
    // 狀態選單
    private void SetStatusOptions()
    {
        ViewBag.StatusOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "上架", Text = "上架" },
            new SelectListItem { Value = "下架", Text = "下架" },
            new SelectListItem { Value = "預購", Text = "預購" },
            new SelectListItem { Value = "補貨中", Text = "補貨中，貨到提醒我" }
        };
    }

    // 供應商選單
    private void SetSupplierOptions()
    {
        ViewBag.SupplierOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "雲布紡織有限公司" },
            new SelectListItem { Value = "2", Text = "晨曦服飾工坊" },
            new SelectListItem { Value = "3", Text = "南方棉麻製造" },
            new SelectListItem { Value = "4", Text = "悅色時尚實業" },
            new SelectListItem { Value = "5", Text = "匠心手作坊" },
            new SelectListItem { Value = "6", Text = "北境針織廠" },
            new SelectListItem { Value = "7", Text = "日晴布行" },
            new SelectListItem { Value = "8", Text = "暖陽成衣廠" },
            new SelectListItem { Value = "9", Text = "青禾織造有限公司" },
            new SelectListItem { Value = "10", Text = "彩羽服飾有限公司" }
        };
    }

    // 供應商選單
    private void SetCategoryOptions()
    {
        ViewBag.CategoryOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "上衣" },
            new SelectListItem { Value = "2", Text = "褲裝" },
            new SelectListItem { Value = "3", Text = "洋裝" },
            new SelectListItem { Value = "4", Text = "外套" },
            new SelectListItem { Value = "5", Text = "裙裝" },
            new SelectListItem { Value = "6", Text = "鞋包配件" },
            new SelectListItem { Value = "7", Text = "運動服飾" },
            new SelectListItem { Value = "8", Text = "內著睡衣" },
            new SelectListItem { Value = "9", Text = "童裝" },
            new SelectListItem { Value = "10", Text = "其他尺碼" }
        };
    }

    public ProductController(CLOthingsContext context)
    {
        _context = context;
    }
    
    // GET: PRODUCTS
    public async Task<IActionResult> Index()    
    {
        // 從資料庫撈出原始的Product資料
        var viewModels = await _context.Products.Select(p => new ProductViewModel //用LINQ的select把每筆Product轉成一筆ProductViewModel，逐欄賦值
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            SupplierId = p.SupplierId,
            ProductCategoryId = p.ProductCategoryId,
            Description = p.Description,
            Price=p.Price,
            Status=p.Status,
            SalesStartDate=p.SalesStartDate,
            SalesEndDate=p.SalesEndDate,
            // 取這件商品的第一張圖,沒有就是 null
            CoverImgFile = p.ProductImg
                .OrderBy(img => img.ProductImgId)   // 換成你想排序的欄位
                .Select(img => img.ProductImgFile)
                .FirstOrDefault()

        }).ToListAsync();
        
        return View(viewModels);
    }

    // GET: PRODUCTS/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.ProductCategory) // 記得要 Include，不然 Navigation Property 不會自動載入
            .Include(p => p.ProductImg)      // 圖片
            .Include(p => p.ProductSpecifications)  // 規格
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null) return NotFound();

        var viewModel = new ProductViewModel
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            SupplierId = product.SupplierId,
            ProductCategoryId = product.ProductCategoryId,
            Description = product.Description,
            Price = product.Price,
            Status = product.Status,
            SalesStartDate = product.SalesStartDate,
            SalesEndDate = product.SalesEndDate,
            ExistingImages = product.ProductImg.Select(img => new ExistingProductImgViewModel
            {
                ProductImgId = img.ProductImgId,
                ProductImgFile = img.ProductImgFile
            }).ToList(),
            ProductSpecifications = product.ProductSpecifications.Select(spec => new ProductSpecificationViewModel
            {
                Size = spec.Size,
                Color = spec.Color,
                Inventory = spec.Inventory,
                UnitOnOrder = spec.UnitOnOrder,
                ReorderLevel = spec.ReorderLevel
            }).ToList()
        };

        return View(viewModel);
    }

    // GET: PRODUCTS/Create
    public IActionResult Create()
    {
        SetStatusOptions();
        SetSupplierOptions();
        SetCategoryOptions();
        return View();
    }

    // POST: PRODUCTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        if (!ModelState.IsValid)
        {
            //foreach (var kvp in ModelState)
            //{
            //    foreach (var err in kvp.Value.Errors)
            //    {
            //        ModelState.AddModelError(string.Empty, $"{kvp.Key}: {err.ErrorMessage}");
            //    }
            //}

            SetStatusOptions();
            SetSupplierOptions();
            SetCategoryOptions();
            return View(model);
        }

        var product = new Product
        { 
            ProductName = model.ProductName,
            SupplierId = model.SupplierId.Value,
            ProductCategoryId = model.ProductCategoryId.Value,
            Description = model.Description,
            Price = model.Price.Value,
            Status = model.Status,
            SalesStartDate = model.SalesStartDate,
            SalesEndDate = model.SalesEndDate,
        };
        // 商品規格：ViewModel → Entity，欄位一一對應
        if (model.ProductSpecifications != null)
        {
            foreach (var spec in model.ProductSpecifications)
            {
                // 整列都沒填就跳過
                if (string.IsNullOrWhiteSpace(spec.Size) 
                    && string.IsNullOrWhiteSpace(spec.Color)
                    && spec.Inventory == null 
                    && spec.UnitOnOrder == null 
                    && spec.ReorderLevel == null)
                    continue;

                product.ProductSpecifications.Add(new ProductSpecification
                {
                    Size = spec.Size,
                    Color = spec.Color,
                    Inventory = spec.Inventory ?? 0,
                    UnitOnOrder = spec.UnitOnOrder,
                    ReorderLevel = spec.ReorderLevel ?? 0
                });
                //product.ProductSpecifications.Add(new ProductSpecification
                //{
                //    Size = spec.Size,
                //    Color = spec.Color,
                //    Inventory = spec.Inventory.Value,
                //    UnitOnOrder = spec.UnitOnOrder,
                //    ReorderLevel = spec.ReorderLevel.Value
                //});
            }
        }

        // 商品圖片：先實際存檔，再包成 ProductImg 實體加入集合
        if (model.ProductImg != null)
        {

            foreach (var file in model.ProductImg)
            {

                if (file.Length > 0)
                {
                    var extension = Path.GetExtension(file.FileName);
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var saveFolder = Path.Combine("wwwroot", "imgs", "products");
                    Directory.CreateDirectory(saveFolder);

                    var savePath = Path.Combine(saveFolder, fileName);


                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }



                    product.ProductImg.Add(new ProductImg   // ← 改成 ProductImg（複數）
                    {
                        ProductImgFile = fileName
                    });


                }
            }
        }

       
        _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        
        
    }

    // GET: PRODUCTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.ProductCategory) // 分類集合
            .Include(p => p.ProductImg)  // 圖片集合
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound();
        }

        SetStatusOptions();
        SetSupplierOptions();
        SetCategoryOptions();

        var viewModel = new ProductViewModel
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            SupplierId = product.SupplierId,
            ProductCategoryId = product.ProductCategoryId,
            Description = product.Description,
            Price = product.Price,
            Status = product.Status,
            SalesStartDate = product.SalesStartDate,
            SalesEndDate = product.SalesEndDate,
            ExistingImages = product.ProductImg.Select(img => new ExistingProductImgViewModel
            {
                ProductImgId = img.ProductImgId,
                ProductImgFile = img.ProductImgFile
            }).ToList()
        };

        return View(viewModel);
    }

    // POST: PRODUCTS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, ProductViewModel model)
    {
        if (id != model.ProductId)
            return NotFound();

        if (ModelState.IsValid)
        {
            SetStatusOptions();
            SetSupplierOptions();
            SetCategoryOptions();
            try
            {
                // 用 id 從資料庫「查出」真正的 Entity，記得 Include 圖片集合
                var product = await _context.Products
                    .Include(p => p.ProductImg)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    return NotFound();
                }

                // 把 ViewModel 的值，一個一個「複製」到 Entity 上
                product.ProductName = model.ProductName;
                product.SupplierId = model.SupplierId.Value;
                product.ProductCategoryId = model.ProductCategoryId.Value;
                product.Description = model.Description;
                product.Price = model.Price.Value;
                product.Status = model.Status;
                product.SalesStartDate = model.SalesStartDate;
                product.SalesEndDate = model.SalesEndDate;

                // 處理刪除勾選的圖片
                if (model.ImagesToDelete != null && model.ImagesToDelete.Any())
                {
                    var imgsToRemove = product.ProductImg
                        .Where(img => model.ImagesToDelete.Contains(img.ProductImgId))
                        .ToList();

                    foreach (var img in imgsToRemove)
                    {
                        // 順便刪除實體檔案
                        var filePath = Path.Combine("wwwroot", "imgs", "products", img.ProductImgFile);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        product.ProductImg.Remove(img);
                        _context.ProductImgs.Remove(img);
                       
                    }
                }

                // 處理新上傳的圖片
                if (model.ProductImg != null)
                {
                    foreach (var file in model.ProductImg)
                    {
                        if (file.Length > 0)
                        {
                            var ext = Path.GetFileName(file.FileName);
                            var fileName = $"{Guid.NewGuid()}{ext}";
                            var saveFolder = Path.Combine("wwwroot", "imgs", "products");
                            Directory.CreateDirectory(saveFolder);
                            var savePath = Path.Combine(saveFolder, fileName);

                            using (var stream = new FileStream(savePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            product.ProductImg.Add(new ProductImg
                            {
                                ProductImgFile = fileName
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(model.ProductId))
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
        return View(model);
    }

    // GET: PRODUCTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var product = await _context.Products
            .Include(p => p.ProductCategory)
            .Include(p => p.ProductImg)      // 圖片
            .Include(p => p.ProductSpecifications)  // 規格
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null) return NotFound();

        var viewModel = new ProductViewModel
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            SupplierId = product.SupplierId,
            ProductCategoryId = product.ProductCategoryId,
            Description = product.Description,
            Price = product.Price,
            Status = product.Status,
            SalesStartDate = product.SalesStartDate,
            SalesEndDate = product.SalesEndDate,
            ExistingImages = product.ProductImg.Select(img => new ExistingProductImgViewModel
            {
                ProductImgId = img.ProductImgId,
                ProductImgFile = img.ProductImgFile
            }).ToList(),
            ProductSpecifications = product.ProductSpecifications.Select(spec => new ProductSpecificationViewModel
            {
                Size = spec.Size,
                Color = spec.Color,
                Inventory = spec.Inventory,
                UnitOnOrder = spec.UnitOnOrder,
                ReorderLevel = spec.ReorderLevel
            }).ToList()
        };

        return View(viewModel);
    }

    // POST: PRODUCTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var product = await _context.Products
            .Include(p => p.ProductImg)
            .Include(p => p.ProductSpecifications)
            .Include(p => p.CustomerFavorites)
            .Include(p => p.PostTaggedProducts)
            .Include(p => p.SellerStatistics)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product != null)
        {
            var specIds = product.ProductSpecifications.Select(s => s.ProductSpecificationId).ToList();

            var carts = await _context.Carts.Where(c => specIds.Contains(c.ProductSpecificationId)).ToListAsync();
            _context.Carts.RemoveRange(carts);

            var orderDetails = await _context.OrderDetails.Where(o => specIds.Contains(o.ProductSpecificationId)).ToListAsync();
            _context.OrderDetails.RemoveRange(orderDetails);

            _context.ProductSpecifications.RemoveRange(product.ProductSpecifications);
            _context.ProductImgs.RemoveRange(product.ProductImg);
            _context.CustomerFavorites.RemoveRange(product.CustomerFavorites);
            _context.PostTaggedProducts.RemoveRange(product.PostTaggedProducts);
            _context.SellerStatistics.RemoveRange(product.SellerStatistics);

            _context.Products.Remove(product);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProductExists(int? productid)
    {
        return _context.Products.Any(e => e.ProductId == productid);
    }
}
