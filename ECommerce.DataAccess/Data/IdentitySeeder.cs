using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.DataAccess.Identity
{
    /// <summary>
    /// Database seeding operations for roles and default admin user.
    /// Implements idempotent seeding with secure password handling via:
    /// 1. Environment variables (highest priority)
    /// 2. User secrets (second priority)
    /// 3. Configuration fallback (lowest priority)
    /// </summary>
    public class IdentitySeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IdentitySeeder> _logger;

        private static class ConfigKeys
        {
            public const string AdminEmail = "AdminUser:Email";
            public const string AdminPassword = "AdminUser:Password";
            public const string AdminFirstName = "AdminUser:FirstName";
            public const string AdminLastName = "AdminUser:LastName";
        }

        private static class DefaultValues
        {
            public const string FirstName = "System";
            public const string LastName = "Administrator";
        }

        public IdentitySeeder(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ILogger<IdentitySeeder> logger)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Seeds all application roles and default admin user (idempotent).
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting Identity seeding operation");

            try
            {
                await SeedRolesAsync();
                await SeedAdminUserAsync();

                _logger.LogInformation("Identity seeding completed successfully");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"Identity seeding failed with configuration error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Identity seeding failed with unexpected error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates application roles if they don't exist (idempotent).
        /// </summary>
        private async Task SeedRolesAsync()
        {
            _logger.LogInformation("Seeding application roles");

            foreach (var roleName in ApplicationRoleNames.All)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);

                if (!roleExists)
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Role '{roleName}' created successfully");
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
                    }
                }
                else
                {
                    _logger.LogDebug($"Role '{roleName}' already exists");
                }
            }
        }

        /// <summary>
        /// Creates default admin user if it doesn't exist, and ensures it has Admin role (idempotent).
        /// </summary>
        private async Task SeedAdminUserAsync()
        {
            _logger.LogInformation("Seeding default admin user");

            var adminEmail = GetConfigurationValue(ConfigKeys.AdminEmail);

            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                throw new InvalidOperationException(
                    $"Configuration key '{ConfigKeys.AdminEmail}' is required for admin seeding. " +
                    $"Set it via User Secrets, Environment Variable, or appsettings.json");
            }

            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                await CreateAdminUserAsync(adminEmail);
            }
            else
            {
                await EnsureAdminRoleAsync(existingAdmin);
            }
        }

        /// <summary>
        /// Creates a new admin user with provided email and configured password.
        /// </summary>
        private async Task CreateAdminUserAsync(string adminEmail)
        {
            _logger.LogInformation($"Creating new admin user with email: {adminEmail}");

            var adminPassword = GetConfigurationValue(ConfigKeys.AdminPassword);

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    $"Configuration key '{ConfigKeys.AdminPassword}' is required for admin user creation. " +
                    $"Set it via User Secrets, Environment Variable, or appsettings.json (not recommended)");
            }

            var adminFirstName = GetConfigurationValue(ConfigKeys.AdminFirstName) ?? DefaultValues.FirstName;
            var adminLastName = GetConfigurationValue(ConfigKeys.AdminLastName) ?? DefaultValues.LastName;

            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = adminFirstName,
                LastName = adminLastName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(adminUser, adminPassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }

            _logger.LogInformation($"Admin user '{adminEmail}' created successfully");

            var roleResult = await _userManager.AddToRoleAsync(adminUser, ApplicationRoleNames.Admin);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to assign Admin role: {errors}");
            }

            _logger.LogInformation($"Admin role assigned to user '{adminEmail}'");
        }

        /// <summary>
        /// Ensures that an existing user has the Admin role (idempotent).
        /// </summary>
        private async Task EnsureAdminRoleAsync(ApplicationUser adminUser)
        {
            _logger.LogInformation($"Admin user '{adminUser.Email}' already exists, verifying role assignment");

            var hasAdminRole = await _userManager.IsInRoleAsync(adminUser, ApplicationRoleNames.Admin);

            if (!hasAdminRole)
            {
                _logger.LogWarning($"Admin user '{adminUser.Email}' missing Admin role, assigning it now");

                var roleResult = await _userManager.AddToRoleAsync(adminUser, ApplicationRoleNames.Admin);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to assign Admin role: {errors}");
                }

                _logger.LogInformation($"Admin role assigned to user '{adminUser.Email}'");
            }
            else
            {
                _logger.LogDebug($"Admin user '{adminUser.Email}' already has Admin role");
            }
        }

        /// <summary>
        /// Gets configuration value with priority: Environment Variable → User Secrets → appsettings.
        /// </summary>
        private string? GetConfigurationValue(string key)
        {
            // Environment variable (highest priority)
            var envValue = Environment.GetEnvironmentVariable(key.Replace(":", "_").ToUpperInvariant());
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                _logger.LogDebug($"Configuration '{key}' loaded from environment variable");
                return envValue;
            }

            // Configuration (User Secrets or appsettings)
            var configValue = _configuration[key];
            if (!string.IsNullOrWhiteSpace(configValue))
            {
                _logger.LogDebug($"Configuration '{key}' loaded from secrets/config");
                return configValue;
            }

            return null;
        }
    }
}