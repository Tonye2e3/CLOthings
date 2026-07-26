using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            // 改從資料庫讀取真實資料
            var orders = await _context.GroupOrders.ToListAsync();
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
                .FirstOrDefaultAsync(m => m.GroupOrderId == id);

            if (grouporder == null)
            {
                return NotFound();
            }

            return View(grouporder);
        }

        // GET: GroupOrder/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GroupOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupOrderId,UserId,Status,TotalPrice,OrderDate,ShipperDate,GroupShipperId,PickupMethod,ShipName,ShipAddress,ShipPhone,Freight,PaymentMethodId")] GroupOrder grouporder)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grouporder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
    }
}