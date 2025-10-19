using System.ComponentModel.DataAnnotations;
namespace EventTickets.Models;
public class Category
{
    public int Id { get; set; }
    [Required, StringLength(50)] public string Name { get; set; } = string.Empty;
    [StringLength(200)] public string? Description { get; set; }
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
