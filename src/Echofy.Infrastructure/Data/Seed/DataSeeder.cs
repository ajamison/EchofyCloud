using Echofy.Domain.Entities;
using Echofy.Domain.Enums;
using Echofy.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Echofy.Infrastructure.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);

        var clients = await SeedClientsAsync(db);
        await SeedUsersAsync(userManager, clients);
        var companies = await SeedCompaniesAsync(db, clients);

        if (!await db.Customers.AnyAsync())
        {
            var categories = await SeedCategoriesAsync(db);
            var products = SeedProducts(categories);
            db.Products.AddRange(products);
            await db.SaveChangesAsync();

            var coupons = SeedCoupons();
            db.Coupons.AddRange(coupons);
            await db.SaveChangesAsync();

            var customers = SeedCustomers(products);
            db.Customers.AddRange(customers);
            await db.SaveChangesAsync();

            var leads = SeedLeads();
            db.Leads.AddRange(leads);
            await db.SaveChangesAsync();

            var deals = SeedDeals(leads);
            db.Deals.AddRange(deals);
            await db.SaveChangesAsync();

            var contacts = SeedContacts();
            db.Contacts.AddRange(contacts);
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "SuperAdmin", "SuperUser", "Admin", "Manager", "Sales", "Support", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task<Dictionary<string, Client>> SeedClientsAsync(ApplicationDbContext db)
    {
        var clientMap = new Dictionary<string, Client>();

        var definitions = new[]
        {
            new { Slug = "echofy",    Name = "Echofy",    ECommerce = true,  Crm = true,  Kanban = true,  Calendar = true,  Chat = true  },
            new { Slug = "acme-corp", Name = "Acme Corp", ECommerce = true,  Crm = true,  Kanban = true,  Calendar = false, Chat = false },
            new { Slug = "techstart", Name = "TechStart", ECommerce = false, Crm = true,  Kanban = true,  Calendar = true,  Chat = true  }
        };

        foreach (var def in definitions)
        {
            var existing = await db.Clients.FirstOrDefaultAsync(c => c.Slug == def.Slug);
            if (existing is null)
            {
                existing = new Client
                {
                    Name = def.Name,
                    Slug = def.Slug,
                    HasECommerce = def.ECommerce,
                    HasCrm = def.Crm,
                    HasKanban = def.Kanban,
                    HasCalendar = def.Calendar,
                    HasChat = def.Chat,
                    IsActive = true
                };
                db.Clients.Add(existing);
                await db.SaveChangesAsync();
            }
            clientMap[def.Slug] = existing;
        }

        return clientMap;
    }

    private static async Task SeedUsersAsync(UserManager<AppUser> userManager, Dictionary<string, Client> clients)
    {
        // Super-level users have no client (they span all tenants)
        var superUsers = new[]
        {
            new { Email = "superadmin@echofy.dev", Password = "SuperAdmin@1234!", FullName = "Super Admin", Role = "SuperAdmin" },
            new { Email = "superuser@echofy.dev",  Password = "SuperUser@1234!",  FullName = "Super User",  Role = "SuperUser"  },
        };

        foreach (var def in superUsers)
        {
            var existing = await userManager.FindByEmailAsync(def.Email);
            if (existing is null)
            {
                var user = new AppUser
                {
                    UserName = def.Email,
                    Email = def.Email,
                    EmailConfirmed = true,
                    FullName = def.FullName,
                    ClientId = null
                };
                var result = await userManager.CreateAsync(user, def.Password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, def.Role);
            }
            else
            {
                if (string.IsNullOrEmpty(existing.FullName))
                {
                    existing.FullName = def.FullName;
                    await userManager.UpdateAsync(existing);
                }
                var currentRoles = await userManager.GetRolesAsync(existing);
                if (!currentRoles.Contains(def.Role))
                {
                    await userManager.RemoveFromRolesAsync(existing, currentRoles);
                    await userManager.AddToRoleAsync(existing, def.Role);
                }
            }
        }

        // Tenant-scoped users
        var tenantUsers = new[]
        {
            new { Email = "admin@echofy.dev",    Password = "Admin@1234!",    FullName = "Admin User",     Role = "Admin",    ClientSlug = "echofy"    },
            new { Email = "manager@echofy.dev",  Password = "Manager@1234!",  FullName = "Alex Manager",   Role = "Manager",  ClientSlug = "acme-corp" },
            new { Email = "sales@echofy.dev",    Password = "Sales@1234!",    FullName = "Sam Sales",      Role = "Sales",    ClientSlug = "acme-corp" },
            new { Email = "support@echofy.dev",  Password = "Support@1234!",  FullName = "Sue Support",    Role = "Support",  ClientSlug = "acme-corp" },
            new { Email = "customer@echofy.dev", Password = "Customer@1234!", FullName = "Chris Customer", Role = "Customer", ClientSlug = "echofy"    }
        };

        foreach (var def in tenantUsers)
        {
            var client = clients[def.ClientSlug];
            var existing = await userManager.FindByEmailAsync(def.Email);

            if (existing is null)
            {
                var user = new AppUser
                {
                    UserName = def.Email,
                    Email = def.Email,
                    EmailConfirmed = true,
                    FullName = def.FullName,
                    ClientId = client.Id
                };
                var result = await userManager.CreateAsync(user, def.Password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, def.Role);
            }
            else
            {
                // Update ClientId/FullName for users seeded before multi-tenancy was added
                bool changed = false;
                if (existing.ClientId is null)
                {
                    existing.ClientId = client.Id;
                    changed = true;
                }
                if (string.IsNullOrEmpty(existing.FullName))
                {
                    existing.FullName = def.FullName;
                    changed = true;
                }
                if (changed)
                    await userManager.UpdateAsync(existing);

                // Ensure the correct role is assigned
                var currentRoles = await userManager.GetRolesAsync(existing);
                if (!currentRoles.Contains(def.Role))
                {
                    await userManager.RemoveFromRolesAsync(existing, currentRoles);
                    await userManager.AddToRoleAsync(existing, def.Role);
                }
            }
        }
    }

    private static async Task<Dictionary<string, Company>> SeedCompaniesAsync(
        ApplicationDbContext db, Dictionary<string, Client> clients)
    {
        var companyMap = new Dictionary<string, Company>();

        var definitions = new[]
        {
            new { Key = "acme-main",   Name = "Acme Supplies Ltd",    ClientSlug = "acme-corp", City = "New York",  Country = "USA",    Email = "info@acme.com",    Phone = "+1 212 000 0001" },
            new { Key = "acme-eu",     Name = "Acme Europe GmbH",     ClientSlug = "acme-corp", City = "Berlin",    Country = "Germany", Email = "eu@acme.com",     Phone = "+49 30 000 0001" },
            new { Key = "echofy-corp", Name = "Echofy Corp",          ClientSlug = "echofy",    City = "San Diego", Country = "USA",    Email = "corp@echofy.dev",  Phone = "+1 619 000 0001" },
        };

        foreach (var def in definitions)
        {
            var client = clients[def.ClientSlug];
            var existing = await db.Companies.FirstOrDefaultAsync(c => c.Name == def.Name && c.ClientId == client.Id);
            if (existing is null)
            {
                existing = new Company
                {
                    ClientId  = client.Id,
                    Name      = def.Name,
                    Email     = def.Email,
                    Phone     = def.Phone,
                    City      = def.City,
                    Country   = def.Country,
                    IsActive  = true,
                    CreatedAt = DateTime.UtcNow,
                };
                db.Companies.Add(existing);
                await db.SaveChangesAsync();
            }
            companyMap[def.Key] = existing;
        }

        return companyMap;
    }

    private static async Task<Dictionary<string, Category>> SeedCategoriesAsync(ApplicationDbContext db)
    {
        var definitions = new[]
        {
            new { Slug = "electronics",  Name = "Electronics",  Description = "Electronic devices and gadgets." },
            new { Slug = "furniture",    Name = "Furniture",    Description = "Office and home furniture." },
            new { Slug = "accessories",  Name = "Accessories",  Description = "Desk accessories and peripherals." },
        };

        var map = new Dictionary<string, Category>();
        foreach (var def in definitions)
        {
            var existing = await db.Categories.FirstOrDefaultAsync(c => c.Slug == def.Slug);
            if (existing is null)
            {
                existing = new Category { Name = def.Name, Slug = def.Slug, Description = def.Description, IsActive = true };
                db.Categories.Add(existing);
            }
            map[def.Slug] = existing;
        }
        await db.SaveChangesAsync();
        return map;
    }

    private static List<Product> SeedProducts(Dictionary<string, Category> cats)
    {
        var electronics = cats["electronics"];
        var furniture   = cats["furniture"];
        var accessories = cats["accessories"];

        return
        [
            new() { Name = "Wireless Headphones Pro",    Price = 129.99m, StockQuantity = 45,  Description = "Premium wireless headphones with active noise cancellation.", Categories = [electronics] },
            new() { Name = "Standing Desk Converter",    Price = 249.00m, StockQuantity = 12,  Description = "Height adjustable desk converter for a healthier workspace.",  Categories = [furniture] },
            new() { Name = "Mechanical Keyboard",        Price = 89.99m,  StockQuantity = 0,   Description = "TKL mechanical keyboard with Cherry MX switches.",             Categories = [electronics] },
            new() { Name = "Ergonomic Mouse",            Price = 59.99m,  StockQuantity = 78,  Description = "Vertical ergonomic mouse to reduce wrist strain.",             Categories = [electronics, accessories] },
            new() { Name = "4K Webcam",                  Price = 149.99m, StockQuantity = 3,   Description = "4K ultra-HD webcam with built-in ring light.",                Categories = [electronics] },
            new() { Name = "Monitor Stand Riser",        Price = 34.99m,  StockQuantity = 0,   Description = "Adjustable monitor stand with USB hub.",                      Categories = [accessories] },
            new() { Name = "Cable Management Kit",       Price = 19.99m,  StockQuantity = 200, Description = "Complete desk cable management solution.",                    Categories = [accessories] },
            new() { Name = "USB-C Hub 7-in-1",           Price = 49.99m,  StockQuantity = 55,  Description = "7-in-1 USB-C hub with HDMI, SD card, and USB 3.0 ports.",    Categories = [electronics, accessories] },
            new() { Name = "Desk Lamp LED",              Price = 39.99m,  StockQuantity = 30,  Description = "LED desk lamp with adjustable color temperature.",             Categories = [accessories] },
            new() { Name = "Office Chair Lumbar Support",Price = 79.99m,  StockQuantity = 20,  Description = "Memory foam lumbar support cushion.",                         Categories = [furniture, accessories] },
        ];
    }

    private static List<Coupon> SeedCoupons() =>
    [
        new() { Code = "SAVE10", CouponType = CouponType.Percentage, Value = 10m, IsActive = true },
        new() { Code = "FLAT20", CouponType = CouponType.FixedCart, Value = 20m, IsActive = true },
        new() { Code = "ITEM5", CouponType = CouponType.FixedProduct, Value = 5m, IsActive = true },
        new() { Code = "WELCOME15", CouponType = CouponType.Percentage, Value = 15m, IsActive = true },
        new() { Code = "SUMMER30", CouponType = CouponType.Percentage, Value = 30m, IsActive = false },
    ];

    private static List<Customer> SeedCustomers(List<Product> products)
    {
        var rng = new Random(42);
        var firstNames = new[] { "Alice", "Bob", "Charlie", "Diana", "Edward", "Fiona", "George", "Hannah", "Ivan", "Julia",
                                  "Kevin", "Laura", "Mike", "Nina", "Oscar", "Paula", "Quinn", "Rachel", "Steve", "Tina" };
        var lastNames = new[] { "Smith", "Jones", "Williams", "Brown", "Taylor", "Davis", "Wilson", "Martinez", "Anderson", "Thomas" };
        var cities = new[] { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego" };
        var countries = new[] { "USA", "Canada", "UK", "Australia" };
        var statuses = Enum.GetValues<PaymentStatus>();
        var fulfillments = Enum.GetValues<FulfillmentStatus>();
        var deliveries = Enum.GetValues<DeliveryType>();

        var customers = new List<Customer>();

        for (int i = 0; i < 50; i++)
        {
            var fn = firstNames[rng.Next(firstNames.Length)];
            var ln = lastNames[rng.Next(lastNames.Length)];
            var customer = new Customer
            {
                FullName = $"{fn} {ln}",
                Email = $"{fn.ToLower()}.{ln.ToLower()}{i}@example.com",
                Phone = $"+1-{rng.Next(200, 999)}-{rng.Next(100, 999)}-{rng.Next(1000, 9999)}",
                JoinedDate = DateTime.UtcNow.AddDays(-rng.Next(30, 730)),
                Address = new Address
                {
                    Street = $"{rng.Next(1, 9999)} Main St",
                    City = cities[rng.Next(cities.Length)],
                    Province = "CA",
                    Country = countries[rng.Next(countries.Length)],
                    PostalCode = $"{rng.Next(10000, 99999)}"
                }
            };

            var orderCount = rng.Next(1, 8);
            for (int j = 0; j < orderCount; j++)
            {
                var product = products[rng.Next(products.Count)];
                var qty = rng.Next(1, 5);
                var order = new Order
                {
                    PaymentStatus = statuses[rng.Next(statuses.Length)],
                    FulfillmentStatus = fulfillments[rng.Next(fulfillments.Length)],
                    DeliveryType = deliveries[rng.Next(deliveries.Length)],
                    CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(1, 365)),
                    Total = product.Price * qty,
                    Items =
                    [
                        new OrderItem { Product = product, Quantity = qty, UnitPrice = product.Price }
                    ]
                };
                customer.Orders.Add(order);
            }

            customers.Add(customer);
        }

        return customers;
    }

    private static List<Lead> SeedLeads()
    {
        var statuses = Enum.GetValues<LeadStatus>();
        var companies = new[] { "Acme Corp", "TechStart", "GreenEnergy", "BuildCo", "FinanceFirst", "RetailPro", "DataDriven", "CloudNine" };
        var names = new[] { "James Miller", "Emma Wilson", "Liam Johnson", "Olivia Davis", "Noah Martinez", "Ava Garcia",
                            "William Rodriguez", "Sophia Lee", "Benjamin Walker", "Mia Hall" };

        var rng = new Random(99);
        var leads = new List<Lead>();

        for (int i = 0; i < 30; i++)
        {
            var name = names[rng.Next(names.Length)];
            leads.Add(new Lead
            {
                FullName = name,
                Email = $"{name.Replace(" ", ".").ToLower()}{i}@business.com",
                Company = companies[rng.Next(companies.Length)],
                Phone = $"+1-555-{rng.Next(1000, 9999)}",
                Status = statuses[rng.Next(statuses.Length)],
                EstimatedValue = rng.Next(500, 50000),
                AssignedTo = "admin@echofy.dev",
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(5, 180))
            });
        }

        return leads;
    }

    private static List<Deal> SeedDeals(List<Lead> leads)
    {
        var stages = Enum.GetValues<DealStage>();
        var rng = new Random(77);
        var deals = new List<Deal>();
        var prefixes = new[] { "Enterprise", "Startup", "Q1", "Annual", "Renewal", "Expansion" };

        for (int i = 0; i < 20; i++)
        {
            var lead = leads[rng.Next(leads.Count)];
            deals.Add(new Deal
            {
                Title = $"{prefixes[rng.Next(prefixes.Length)]} Deal #{i + 1}",
                Lead = lead,
                Stage = stages[rng.Next(stages.Length)],
                Value = rng.Next(1000, 100000),
                ExpectedCloseDate = DateTime.UtcNow.AddDays(rng.Next(7, 90)),
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(1, 60))
            });
        }

        return deals;
    }

    private static List<Contact> SeedContacts() =>
    [
        new() { FirstName = "Sophie", LastName = "Turner", Email = "s.turner@acme.com", Phone = "+1-555-0001", Company = "Acme Corp" },
        new() { FirstName = "James", LastName = "Brown", Email = "j.brown@techstart.io", Phone = "+1-555-0002", Company = "TechStart" },
        new() { FirstName = "Maria", LastName = "Garcia", Email = "m.garcia@greenco.com", Phone = "+1-555-0003", Company = "GreenEnergy" },
        new() { FirstName = "Chris", LastName = "Lee", Email = "c.lee@buildco.com", Phone = "+1-555-0004", Company = "BuildCo" },
        new() { FirstName = "Anna", LastName = "Kim", Email = "a.kim@financefirst.com", Phone = "+1-555-0005", Company = "FinanceFirst" },
    ];
}
