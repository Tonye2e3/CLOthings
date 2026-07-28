using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CLOthings.Models;

namespace CLOthings.Controllers
{
    public class GroupOrderController : Controller
    {
        private readonly CLOthingsContext _context;

        public GroupOrderController(CLOthingsContext context)
        {
            _context = context;
        }

        // GET: GroupOrder
        public async Task<IActionResult> Index()
        {
            var orders = await _context.GroupOrders
                .Include(o => o.GroupOrderDetails)
                    .ThenInclude(d => d.GroupProduct)
                .ToListAsync();

            return View(orders);
        }

        // GET: GroupOrder/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grouporder = await _context.GroupOrders
                .Include(o => o.GroupOrderDetails)
                    .ThenInclude(d => d.GroupProduct)
                .Include(o => o.PaymentMethod)
                .Include(o => o.GroupShipper)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.GroupOrderId == id);

            if (grouporder == null)
            {
                return NotFound();
            }

            return View(grouporder);
        }

        // GET: GroupOrder/Create
        public async Task<IActionResult> Create()
        {
            int currentUserId = 1;

            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out int parsedId))
                {
                    currentUserId = parsedId;
                }
            }

            // 載入付款方式與商品選單
            await PopulateDropDownLists();

            var model = new GroupOrder
            {
                UserId = currentUserId,
                OrderDate = DateTime.Now,
                Freight = 0,
                Status = "處理中",
                PickupMethod = "宅配到府"
            };

            return View(model);
        }

        // POST: GroupOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupOrderId,UserId,Status,TotalPrice,OrderDate,ShipperDate,GroupShipperId,PickupMethod,ShipName,ShipAddress,ShipPhone,Freight,PaymentMethodId")] GroupOrder grouporder, int productId, int quantity)
        {
            if (grouporder.ShipperDate.HasValue && grouporder.ShipperDate < grouporder.OrderDate)
            {
                ModelState.AddModelError("ShipperDate", "預計/實際出貨日期不能早於訂單日期！");
            }

            if (productId <= 0)
            {
                ModelState.AddModelError("", "請選擇商品！");
            }

            if (ModelState.IsValid)
            {
                // 1. 儲存主訂單
                _context.Add(grouporder);
                await _context.SaveChangesAsync();

                // 2. 儲存訂單商品明細
                var orderDetail = new GroupOrderDetail
                {
                    GroupOrderId = grouporder.GroupOrderId,
                    GroupProductId = productId,
                    Quantity = quantity > 0 ? quantity : 1
                };

                _context.GroupOrderDetails.Add(orderDetail);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await PopulateDropDownLists(grouporder.PaymentMethodId, productId);
            return View(grouporder);
        }

        // GET: GroupOrder/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grouporder = await _context.GroupOrders.FindAsync(id);
            if (grouporder == null)
            {
                return NotFound();
            }

            await PopulateDropDownLists(grouporder.PaymentMethodId);
            return View(grouporder);
        }

        // POST: GroupOrder/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupOrderId,UserId,Status,TotalPrice,OrderDate,ShipperDate,GroupShipperId,PickupMethod,ShipName,ShipAddress,ShipPhone,Freight,PaymentMethodId")] GroupOrder grouporder)
        {
            if (id != grouporder.GroupOrderId)
            {
                return NotFound();
            }

            if (grouporder.ShipperDate.HasValue && grouporder.ShipperDate < grouporder.OrderDate)
            {
                ModelState.AddModelError("ShipperDate", "預計/實際出貨日期不能早於訂單日期！");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grouporder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupOrderExists(grouporder.GroupOrderId))
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

            await PopulateDropDownLists(grouporder.PaymentMethodId);
            return View(grouporder);
        }

        // GET: GroupOrder/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grouporder = await _context.GroupOrders
                .FirstOrDefaultAsync(m => m.GroupOrderId == id);

            if (grouporder == null)
            {
                return NotFound();
            }

            return View(grouporder);
        }

        // POST: GroupOrder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grouporder = await _context.GroupOrders.FindAsync(id);
            if (grouporder != null)
            {
                _context.GroupOrders.Remove(grouporder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupOrderExists(int id)
        {
            return _context.GroupOrders.Any(e => e.GroupOrderId == id);
        }

        // 載入付款方式與商品選單
        private async Task PopulateDropDownLists(object? selectedPaymentMethod = null, object? selectedProduct = null)
        {
            var paymentMethods = await _context.GroupPaymentMethods.ToListAsync();
            ViewData["PaymentMethodId"] = new SelectList(
                paymentMethods,
                "GroupPaymentMethodId",
                "CardBrand",
                selectedPaymentMethod
            );

            var products = await _context.GroupProducts.ToListAsync();
            ViewData["GroupProductId"] = new SelectList(
                products,
                "GroupProductId",
                "ProductName", // 若商品名稱欄位不是 ProductName，請對應修改為你的商品名稱欄位
                selectedProduct
            );
        }
    }
}