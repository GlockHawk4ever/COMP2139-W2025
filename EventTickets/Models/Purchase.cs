using System.ComponentModel.DataAnnotations;

namespace EventTickets.Models;

public class Purchase
{
    public int Id { get; set; }

    [Required]
    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }

    // Guest/contact info (optional once Identity is used)
    public string CustomerName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public DateTime PurchasedAt { get; set; } = DateTime.Now;

    [Range(0, 1000000)]
    public decimal Total { get; set; }

    // Link purchase to logged-in user (for dashboard)
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    // Optional 1–5 star rating for past purchases
    [Range(1, 5)]
    public int? Rating { get; set; }
}
