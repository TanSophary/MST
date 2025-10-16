using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MST.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using MST.Data;

namespace MST.Controllers;

[Authorize(Roles = "Admin")]
public class ProjectController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ProjectController(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        var projects = await _context.Projects.ToListAsync();
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
    public async Task<IActionResult> AddProject([FromForm] Project model, IFormFile Image)
    {
        if (ModelState.IsValid)
        {
            model.CreateDate = DateTime.Now;
            if (Image != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "Uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Image.CopyToAsync(stream);
                }
                model.ImagePath = fileName;
            }

            _context.Projects.Add(model);
            await _context.SaveChangesAsync();
            return Ok();
        }
        return BadRequest();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProject([FromForm] Project model, IFormFile Image)
    {
        if (ModelState.IsValid)
        {
            var existingProject = await _context.Projects.FindAsync(model.Id);
            if (existingProject == null) return NotFound();

            existingProject.Name = model.Name;
            existingProject.Location = model.Location;
            existingProject.Description = model.Description;
            existingProject.Status = model.Status;
            // CreateDate remains unchanged

            if (Image != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "Uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Image.CopyToAsync(stream);
                }
                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingProject.ImagePath))
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath, "Uploads", existingProject.ImagePath);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                existingProject.ImagePath = fileName;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        return BadRequest();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return NotFound();

        if (!string.IsNullOrEmpty(project.ImagePath))
        {
            var imagePath = Path.Combine(_environment.WebRootPath, "Uploads", project.ImagePath);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return Ok();
    }
}