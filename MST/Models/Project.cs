using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MST.Data;
using MST.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace MST.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Location { get; set; } = null!;

        // Optional
        public string? Description { get; set; }

        [Required]
        public string Status { get; set; } = null!;

        // Optional image path (nullable)
        public string? Thumbnail { get; set; }
        public string? ImageList { get; set; }
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
    }
}