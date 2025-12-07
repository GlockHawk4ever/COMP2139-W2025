using System.ComponentModel.DataAnnotations;

namespace EventTickets.Models;

public class Event
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Display(Name = "Date & Time")]
    public DateTime? DateTime { get; set; }

    [Range(0, 100000), Display(Name = "Ticket Price")]
    public decimal Price { get; set; }

    [Range(0, 100000), Display(Name = "Available Tickets")]
    public int AvailableTickets { get; set; }

    // Optional organizer for Assignment 2 dashboard/analytics
    public string? OrganizerId { get; set; }
    public ApplicationUser? Organizer { get; set; }

    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
