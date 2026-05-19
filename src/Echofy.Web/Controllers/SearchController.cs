using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Web.Controllers;

[Authorize]
public class SearchController(
    IProductService products,
    ICustomerService customers,
    ILeadService leads,
    IDealService deals) : Controller
{
    [HttpGet("/search")]
    public async Task<IActionResult> Query(string? q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Json(new { groups = Array.Empty<object>() });

        var term = q.Trim();

        // Products and Customers support server-side search; Leads/Deals are loaded all then filtered.
        var productsTask  = products.GetAllAsync(search: term, ct: ct);
        var customersTask = customers.GetAllAsync(null, term, ct);
        var leadsTask     = leads.GetAllAsync(ct);
        var dealsTask     = deals.GetAllAsync(ct);

        await Task.WhenAll(productsTask, customersTask, leadsTask, dealsTask);

        var productItems = productsTask.Result
            .Take(5)
            .Select(p => new
            {
                text = p.Name,
                sub  = string.Join(", ", p.CategoryNames),
                url  = Url.Action("Details", "Products", new { id = p.Id })
            })
            .ToList();

        var customerItems = customersTask.Result
            .Take(5)
            .Select(c => new
            {
                text = c.FullName,
                sub  = c.Email,
                url  = Url.Action("Details", "Customers", new { id = c.Id })
            })
            .ToList();

        var leadItems = leadsTask.Result
            .Where(l => l.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)
                     || (l.Company?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                     || l.Email.Contains(term, StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .Select(l => new
            {
                text = l.FullName,
                sub  = l.Company ?? l.Email,
                url  = Url.Action("Details", "Leads", new { id = l.Id })
            })
            .ToList();

        var dealItems = dealsTask.Result
            .Where(d => d.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                     || d.LeadName.Contains(term, StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .Select(d => new
            {
                text = d.Title,
                sub  = d.LeadName,
                url  = Url.Action("Details", "Deals", new { id = d.Id })
            })
            .ToList();

        var groups = new List<object>();
        if (productItems.Count  > 0) groups.Add(new { label = "Products",  items = productItems  });
        if (customerItems.Count > 0) groups.Add(new { label = "Customers", items = customerItems });
        if (leadItems.Count     > 0) groups.Add(new { label = "Leads",     items = leadItems     });
        if (dealItems.Count     > 0) groups.Add(new { label = "Deals",     items = dealItems     });

        return Json(new { groups });
    }
}
