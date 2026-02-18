using ECommerce.Models.Identity;
using ECommerce.Models.Models;

namespace ECommerce.Models.ViewModels
{
    public class AdminDashboardVM
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<ApplicationUser> RecentUsers { get; set; } = new();
        public List<UserListViewModel> UserListViewModels { get; set; } = new();
    }
}
