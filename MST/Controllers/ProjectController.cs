using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MST.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using MST.Data;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json; // For JSON serialization

namespace MST.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProjectController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(AppDbContext context, IWebHostEnvironment environment, ILogger<ProjectController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Admin Page";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _context.Projects.OrderByDescending(p => p.StartDate).ToListAsync();
            return Json(projects);
        }

        [HttpGet]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();
            return Json(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProject([FromForm] Project model, IFormFile? Image, IFormFileCollection? ImageList)
        {
            if (model.StartDate.HasValue && model.EndDate.HasValue && model.EndDate < model.StartDate)
            {
                return BadRequest(new { Errors = new[] { "End Date cannot be before Start Date." } });
            }
            if (model == null)
            {
                return BadRequest(new { Errors = new[] { "Project model is null" } });
            }

            ModelState.Remove(nameof(model.Thumbnail));
            ModelState.Remove(nameof(model.Id));
            ModelState.Remove(nameof(model.ImageList));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Errors = errors });
            }

            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Handle Thumbnail (single image)
                if (Image != null && Image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(Image.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }
                    model.Thumbnail = fileName;
                }
                else
                {
                    model.Thumbnail = null;
                }

                // Handle ImageList (multiple images)
                var imageList = new List<string>();
                if (ImageList != null && ImageList.Count > 0)
                {
                    foreach (var file in ImageList)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadsFolder, fileName);
                            await using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            imageList.Add(fileName);
                        }
                    }
                }
                model.ImageList = imageList.Any() ? JsonSerializer.Serialize(imageList) : null;

                model.Id = 0; // Ensure EF treats as new entity
                _context.Projects.Add(model);
                await _context.SaveChangesAsync();
                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to add project: {ex.Message}" });
            }
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProject([FromForm] Project model, IFormFile? Image, IFormFileCollection? ImageList)
        {
            if (model.StartDate.HasValue && model.EndDate.HasValue && model.EndDate < model.StartDate)
            {
                return BadRequest(new { Errors = new[] { "End Date cannot be before Start Date." } });
            }
            if (model == null)
                return BadRequest(new { Errors = new[] { "Project model is null" } });

            ModelState.Remove(nameof(model.Thumbnail));
            ModelState.Remove(nameof(model.ImageList));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Errors = errors });
            }

            var existingProject = await _context.Projects.FindAsync(model.Id);
            if (existingProject == null) return NotFound(new { Error = "Project not found" });

            try
            {
                existingProject.Name = model.Name;
                existingProject.Location = model.Location;
                existingProject.Description = model.Description;
                existingProject.Status = model.Status;
                existingProject.StartDate = model.StartDate;
                existingProject.EndDate = model.EndDate;

                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Handle Thumbnail
                if (Image != null && Image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(Image.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }

                    // Delete old thumbnail if exists
                    if (!string.IsNullOrEmpty(existingProject.Thumbnail))
                    {
                        var oldImagePath = Path.Combine(uploadsFolder, existingProject.Thumbnail);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    existingProject.Thumbnail = fileName;
                }

                // Handle ImageList - Only update if new images are provided
                if (ImageList != null && ImageList.Any(file => file.Length > 0))
                {
                    // Delete old ImageList images if they exist
                    if (!string.IsNullOrEmpty(existingProject.ImageList))
                    {
                        var oldImageList = JsonSerializer.Deserialize<List<string>>(existingProject.ImageList) ?? new List<string>();
                        foreach (var oldImage in oldImageList)
                        {
                            var oldImagePath = Path.Combine(uploadsFolder, oldImage);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                    }

                    // Add new images to ImageList
                    var imageList = new List<string>();
                    foreach (var file in ImageList)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadsFolder, fileName);
                            await using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            imageList.Add(fileName);
                        }
                    }
                    existingProject.ImageList = imageList.Any() ? JsonSerializer.Serialize(imageList) : null;
                }
                // If no new images are uploaded, do not modify existingProject.ImageList

                await _context.SaveChangesAsync();
                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to update project: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "Uploads");

            // Delete Thumbnail
            if (!string.IsNullOrEmpty(project.Thumbnail))
            {
                var imagePath = Path.Combine(uploadsFolder, project.Thumbnail);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Delete ImageList images
            if (!string.IsNullOrEmpty(project.ImageList))
            {
                var imageList = JsonSerializer.Deserialize<List<string>>(project.ImageList) ?? new List<string>();
                foreach (var image in imageList)
                {
                    var imagePath = Path.Combine(uploadsFolder, image);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true });
        }
    }
}