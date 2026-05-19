namespace Echofy.Application.Navigation;

/// <summary>
/// Static definition of all possible nav items. NavigationService filters this
/// based on the user's role claims and their client's module claims.
/// </summary>
public static class MenuDefinition
{
    public static IReadOnlyList<NavGroupDef> Groups { get; } =
    [
        new NavGroupDef(
            Label: "Home",
            Icon: "pie-chart",
            CollapseId: "nv-home",
            Items:
            [
                new NavItemDef("E-Commerce Dashboard", "Dashboard", "Index", "Dashboard",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin", "Manager"],
                    RequiredModule: "ecommerce"),
                new NavItemDef("CRM Dashboard", "Crm", "Dashboard", "CrmDashboard",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin", "Manager", "Sales"],
                    RequiredModule: "crm")
            ]),

        new NavGroupDef(
            Label: "E-Commerce",
            Icon: "shopping-cart",
            CollapseId: "nv-ecommerce",
            Items:
            [
                new NavItemDef("Products", "Products", "Index", "Products",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin", "Manager"],
                    RequiredModule: "ecommerce"),
                new NavItemDef("Customers", "Customers", "Index", "Customers",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin", "Manager", "Support"],
                    RequiredModule: "ecommerce"),
            ]),

        new NavGroupDef(
            Label: "CRM",
            Icon: "users",
            CollapseId: "nv-crm",
            Items:
            [
                new NavItemDef("Leads", "Leads", "Index", "Leads",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin", "Manager", "Sales"],
                    RequiredModule: "crm"),
                new NavItemDef("Deals", "Deals", "Index", "Deals",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin", "Manager", "Sales"],
                    RequiredModule: "crm"),
                new NavItemDef("Contacts", "Contacts", "Index", "Contacts",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin", "Manager", "Sales"],
                    RequiredModule: "crm")
            ]),

        new NavGroupDef(
            Label: "Apps",
            Icon: "grid",
            CollapseId: "nv-apps",
            Items:
            [
                new NavItemDef("Kanban", "Kanban", "Index", "Kanban",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin", "Manager", "Support"],
                    RequiredModule: "kanban"),
                new NavItemDef("Calendar", "Calendar", "Index", "Calendar",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin", "Manager", "Support"],
                    RequiredModule: "calendar"),
                new NavItemDef("Chat", "Chat", "Index", "Chat",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin", "Manager", "Support"],
                    RequiredModule: "chat")
            ]),

        new NavGroupDef(
            Label: "My Account",
            Icon: "heart",
            CollapseId: "nv-customer",
            Items:
            [
                new NavItemDef("Dashboard", "Customer", "Dashboard", "CustomerDashboard",
                    RequiredRoles: ["Admin", "Manager", "Sales", "Support", "Customer"],
                    RequiredModule: null),
                new NavItemDef("My Favorites", "Customer", "Favorites", "CustomerFavorites",
                    RequiredRoles: ["Admin", "Manager", "Sales", "Support", "Customer"],
                    RequiredModule: null),
                new NavItemDef("My Referrals", "Customer", "Referrals", "CustomerReferrals",
                    RequiredRoles: ["Admin", "Manager", "Sales", "Support", "Customer"],
                    RequiredModule: null)
            ]),

        new NavGroupDef(
            Label: "Admin",
            Icon: "settings",
            CollapseId: "nv-admin",
            Items:
            [
                new NavItemDef("Users", "Admin", "Users", "AdminUsers",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin"],
                    RequiredModule: null),
                new NavItemDef("Clients", "Admin", "Clients", "AdminClients",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin"],
                    RequiredModule: null),
                new NavItemDef("Companies", "Companies", "Index", "AdminCompanies",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin"],
                    RequiredModule: null),
                new NavItemDef("Categories", "Admin", "Categories", "AdminCategories",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin"],
                    RequiredModule: null),
                new NavItemDef("Reviews", "Admin", "Reviews", "AdminReviews",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin"],
                    RequiredModule: null),
                new NavItemDef("Manufacturers", "Admin", "Manufacturers", "AdminManufacturers",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin"],
                    RequiredModule: null),
                new NavItemDef("Units of Measure", "Admin", "UnitsOfMeasure", "AdminUnitsOfMeasure",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin"],
                    RequiredModule: null),
                new NavItemDef("Referrals", "Admin", "Referrals", "AdminReferrals",
                    RequiredRoles: ["SuperAdmin", "SuperUser", "Admin"],
                    RequiredModule: null)
            ])
    ];
}

public record NavGroupDef(
    string Label,
    string Icon,
    string CollapseId,
    IReadOnlyList<NavItemDef> Items);

public record NavItemDef(
    string Text,
    string Controller,
    string Action,
    string ActivePage,
    string[] RequiredRoles,
    string? RequiredModule);
