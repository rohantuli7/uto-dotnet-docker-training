using DashboardApi.Data;
using DashboardApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DashboardApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardContext _context;

    public DashboardController(DashboardContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DashboardItem>>> GetAllItems()
    {
        return await _context.DashboardItems.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DashboardItem>> GetItem(int id)
    {
        var item = await _context.DashboardItems.FindAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        return item;
    }

    [HttpPost]
    public async Task<ActionResult<DashboardItem>> CreateItem(DashboardItem item)
    {
        _context.DashboardItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, DashboardItem item)
    {
        if (id != item.Id)
        {
            return BadRequest();
        }

        _context.Entry(item).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.DashboardItems.Any(e => e.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var item = await _context.DashboardItems.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        _context.DashboardItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
