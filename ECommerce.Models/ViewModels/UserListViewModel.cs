namespace ECommerce.Models.ViewModels
{
    /// <summary>
    /// Kullanıcı listesi için ViewModel.
    /// </summary>
    public class UserListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
