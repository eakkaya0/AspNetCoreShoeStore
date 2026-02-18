namespace ECommerce.Models.ViewModels
{
    /// <summary>
    /// Kullanıcı detay ve rol atama için ViewModel.
    /// </summary>
    public class UserDetailViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<RoleSelectionViewModel> Roles { get; set; } = new List<RoleSelectionViewModel>();
    }

    /// <summary>
    /// Rol seçimi için ViewModel.
    /// </summary>
    public class RoleSelectionViewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
