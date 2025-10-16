using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MST.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using MST.Data;
using Microsoft.AspNetCore.Hosting;

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
        public async Task<IActionResult> AddProject([FromForm] Project model, IFormFile? Image)
        {
            if (model.StartDate.HasValue && model.EndDate.HasValue && model.EndDate < model.StartDate)
            {
                return BadRequest(new { Errors = new[] { "End Date cannot be before Start Date." } });
            }
            if (model == null)
            {
                return BadRequest(new { Errors = new[] { "Project model is null" } });
            }

            ModelState.Remove(nameof(model.ImagePath));
            ModelState.Remove(nameof(model.Id));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Errors = errors });
            }

            try
            {
                //model.CreateDate = DateTime.UtcNow;

                if (Image != null && Image.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "Uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(Image.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }
                    model.ImagePath = fileName;
                }
                else
                {
                    model.ImagePath = null;
                }

                // Ensure Id is 0 so EF treats as new entity
                model.Id = 0;

                _context.Projects.Add(model);
                await _context.SaveChangesAsync();
                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to add project: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProject([FromForm] Project model, IFormFile? Image)
        {
            if (model.StartDate.HasValue && model.EndDate.HasValue && model.EndDate < model.StartDate)
            {
                return BadRequest(new { Errors = new[] { "End Date cannot be before Start Date." } });
            }
            if (model == null)
                return BadRequest(new { Errors = new[] { "Project model is null" } });

            // Do not trust a posted ImagePath; server manages it
            ModelState.Remove(nameof(model.ImagePath));

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


                if (Image != null && Image.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "Uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(Image.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingProject.ImagePath))
                    {
                        var oldImagePath = Path.Combine(uploadsFolder, existingProject.ImagePath);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    existingProject.ImagePath = fileName;
                }

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

            if (!string.IsNullOrEmpty(project.ImagePath))
            {
                var imagePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "Uploads", project.ImagePath);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true });
        }
    }
}
