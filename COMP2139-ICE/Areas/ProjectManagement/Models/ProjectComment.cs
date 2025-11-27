using System;
using System.ComponentModel.DataAnnotations;

namespace COMP2139_ICE.Areas.ProjectManagement.Models
{
    public class ProjectComment
    {
        public int ProjectCommentId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Comment cannot be longer than 500 characters.")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int ProjectId { get; set; }

        public DateTime CreatedDate { get; set; }

        public Project? Project { get; set; }
    }
}
