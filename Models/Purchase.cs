using System.ComponentModel.DataAnnotations;
namespace EventTickets.Models;
public class Purchase
{
    public int Id { get; set; }
    [Required] public int EventId { get; set; }
    public Event? Event { get; set; }
    [Range(1, 1000)] public int Quantity { get; set; }
    [Required, StringLength(100)] public string CustomerName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public DateTime PurchasedAt { get; set; } = DateTime.Now;
    [Range(0, 1000000)] public decimal Total { get; set; }
}
