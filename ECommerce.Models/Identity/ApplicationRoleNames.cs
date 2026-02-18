namespace ECommerce.Models.Identity
{
    public static class ApplicationRoleNames
    {
        public const string Admin = "Admin";
        public const string Employee = "Employee";
        public const string Customer = "Customer";

        public static readonly IReadOnlyList<string> All = new[] { Admin, Employee, Customer };
    }
}
