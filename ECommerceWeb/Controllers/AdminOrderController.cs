using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Models.Models;
using ECommerce.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminOrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Tüm siparişleri listeler
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var orders = await _unitOfWork.Order.GetAllOrdersAsync();
            
            var orderVMs = orders.Select(o => new OrderSummaryVM
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Status = o.Status,
                StatusText = GetStatusText(o.Status),
                StatusColor = GetStatusColor(o.Status),
                TotalAmount = o.TotalAmount,
                ShippingCost = o.ShippingCost,
                GrandTotal = o.GrandTotal,
                IsPaid = o.IsPaid,
                CustomerName = o.IsGuestOrder
                     ? $"{o.GuestFirstName} {o.GuestLastName}".Trim()
                     : $"{o.ApplicationUser?.FirstName} {o.ApplicationUser?.LastName}".Trim(),
                CustomerEmail = o.IsGuestOrder
                     ? (o.GuestEmail ?? string.Empty)
                     : (o.ApplicationUser?.Email ?? string.Empty),
                OrderItems = o.OrderItems.Select(oi => new OrderItemVM
                {
                    Id = oi.Id,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl,
                    VariantInfo = oi.ProductVariant?.Size,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            }).ToList();

            return View(orderVMs);
        }

        /// <summary>
        /// Sipariş detaylarını gösterir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _unitOfWork.Order.GetOrderWithDetailsAsync(id);
            
            if (order == null)
            {
                return RedirectToAction("Index");
            }

            return View(order);
        }

        /// <summary>
        /// Sipariş durumunu günceller
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _unitOfWork.Order.GetAsync(o => o.Id == id);
            
            if (order == null)
            {
                return Json(new { success = false, message = "Sipariş bulunamadı." });
            }

            order.Status = status;
            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();

            return Json(new { 
                success = true, 
                message = "Sipariş durumu güncellendi.",
                statusText = GetStatusText(status),
                statusColor = GetStatusColor(status)
            });
        }

        /// <summary>
        /// Siparişi iptal eder
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _unitOfWork.Order.GetAsync(o => o.Id == id);
            
            if (order == null)
            {
                return Json(new { success = false, message = "Sipariş bulunamadı." });
            }

            order.Status = OrderStatus.Cancelled;
            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();

            return Json(new { 
                success = true, 
                message = "Sipariş iptal edildi.",
                statusText = "İptal Edildi",
                statusColor = "danger"
            });
        }

        private string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Beklemede",
                OrderStatus.Confirmed => "Onaylandı",
                OrderStatus.Preparing => "Hazırlanıyor",
                OrderStatus.Shipped => "Kargoda",
                OrderStatus.Delivered => "Teslim Edildi",
                OrderStatus.Cancelled => "İptal Edildi",
                OrderStatus.Refunded => "İade Edildi",
                _ => "Bilinmeyen"
            };
        }

        private string GetStatusColor(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "warning",
                OrderStatus.Confirmed => "info",
                OrderStatus.Preparing => "primary",
                OrderStatus.Shipped => "success",
                OrderStatus.Delivered => "success",
                OrderStatus.Cancelled => "danger",
                OrderStatus.Refunded => "secondary",
                _ => "secondary"
            };
        }
    }
}
