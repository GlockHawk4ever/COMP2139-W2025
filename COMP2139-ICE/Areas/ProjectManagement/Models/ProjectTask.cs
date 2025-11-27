using System.ComponentModel.DataAnnotations;

namespace COMP2139_ICE.Areas.ProjectManagement.Models
{
    public class ProjectTask
    {
        public int ProjectTaskId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Task title cannot be longer than 100 characters.")]
        [Display(Name = "Task Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Task description cannot be longer than 500 characters.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Task Description")]
        public string? Description { get; set; }

        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        public Project? Project { get; set; }
    }
}
