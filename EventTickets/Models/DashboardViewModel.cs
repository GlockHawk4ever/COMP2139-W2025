using System.Collections.Generic;

namespace EventTickets.Models;

public class DashboardViewModel
{
    public ApplicationUser? CurrentUser { get; set; }

    public List<Purchase> UpcomingTickets { get; set; } = new();
    public List<Purchase> PastPurchases { get; set; } = new();

    public List<Event> MyEvents { get; set; } = new();
    public Dictionary<int, decimal> RevenueByEvent { get; set; } = new();
}
