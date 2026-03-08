using AutoMapper;
using family_registry_backend.Data;
using family_registry_backend.DTOs;
using family_registry_backend.Models;
using family_registry_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace family_registry_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly PdfService _pdfService;

    public EmployeesController(AppDbContext context, IMapper mapper, PdfService pdfService)
    {
        _context = context;
        _mapper = mapper;
        _pdfService = pdfService;
    }

    // GET: api/employees?search=xxx
    [HttpGet]
    public async Task<ActionResult<List<EmployeeResponseDto>>> GetAll([FromQuery] string? search)
    {
        var query = _context.Employees
            .Include(e => e.Spouse)
            .Include(e => e.Children)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(term) ||
                e.NID.ToLower().Contains(term) ||
                e.Department.ToLower().Contains(term));
        }

        var employees = await query.OrderBy(e => e.Name).ToListAsync();
        return Ok(_mapper.Map<List<EmployeeResponseDto>>(employees));
    }

    // GET: api/employees/5
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeResponseDto>> GetById(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Spouse)
            .Include(e => e.Children)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return NotFound(new { message = "Employee not found." });

        return Ok(_mapper.Map<EmployeeResponseDto>(employee));
    }

    // POST: api/employees
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeResponseDto>> Create([FromBody] EmployeeCreateDto dto)
    {
        // Check NID uniqueness
        if (await _context.Employees.AnyAsync(e => e.NID == dto.NID))
            return Conflict(new { message = "An employee with this NID already exists." });

        if (dto.Spouse != null && await _context.Spouses.AnyAsync(s => s.NID == dto.Spouse.NID))
            return Conflict(new { message = "A spouse with this NID already exists." });

        var employee = _mapper.Map<Employee>(dto);

        if (dto.Spouse != null)
        {
            employee.Spouse = _mapper.Map<Spouse>(dto.Spouse);
        }

        if (dto.Children.Any())
        {
            employee.Children = dto.Children.Select(c => _mapper.Map<Child>(c)).ToList();
        }

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Re-fetch with includes
        var created = await _context.Employees
            .Include(e => e.Spouse)
            .Include(e => e.Children)
            .FirstAsync(e => e.Id == employee.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            _mapper.Map<EmployeeResponseDto>(created));
    }

    // PUT: api/employees/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeResponseDto>> Update(int id, [FromBody] EmployeeUpdateDto dto)
    {
        var employee = await _context.Employees
            .Include(e => e.Spouse)
            .Include(e => e.Children)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return NotFound(new { message = "Employee not found." });

        // Check NID uniqueness (exclude current employee)
        if (await _context.Employees.AnyAsync(e => e.NID == dto.NID && e.Id != id))
            return Conflict(new { message = "An employee with this NID already exists." });

        if (dto.Spouse != null)
        {
            if (await _context.Spouses.AnyAsync(s => s.NID == dto.Spouse.NID && s.EmployeeId != id))
                return Conflict(new { message = "A spouse with this NID already exists." });
        }

        // Update basic fields
        _mapper.Map(dto, employee);

        // Update spouse
        if (dto.Spouse != null)
        {
            if (employee.Spouse != null)
            {
                employee.Spouse.Name = dto.Spouse.Name;
                employee.Spouse.NID = dto.Spouse.NID;
            }
            else
            {
                employee.Spouse = _mapper.Map<Spouse>(dto.Spouse);
            }
        }
        else
        {
            if (employee.Spouse != null)
            {
                _context.Spouses.Remove(employee.Spouse);
                employee.Spouse = null;
            }
        }

        // Update children — replace all
        _context.Children.RemoveRange(employee.Children);
        employee.Children = dto.Children.Select(c => _mapper.Map<Child>(c)).ToList();

        await _context.SaveChangesAsync();

        // Re-fetch
        var updated = await _context.Employees
            .Include(e => e.Spouse)
            .Include(e => e.Children)
            .FirstAsync(e => e.Id == id);

        return Ok(_mapper.Map<EmployeeResponseDto>(updated));
    }

    // DELETE: api/employees/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound(new { message = "Employee not found." });

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/employees/export/pdf?search=xxx
    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportTablePdf([FromQuery] string? search)
    {
        var query = _context.Employees
            .Include(e => e.Spouse)
            .Include(e => e.Children)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(term) ||
                e.NID.ToLower().Contains(term) ||
                e.Department.ToLower().Contains(term));
        }

        var employees = await query.OrderBy(e => e.Name).ToListAsync();
        var dtos = _mapper.Map<List<EmployeeResponseDto>>(employees);

        var pdfBytes = _pdfService.GenerateTablePdf(dtos);
        return File(pdfBytes, "application/pdf", "Employee_List.pdf");
    }

    // GET: api/employees/5/export/cv
    [HttpGet("{id}/export/cv")]
    public async Task<IActionResult> ExportCvPdf(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Spouse)
            .Include(e => e.Children)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return NotFound(new { message = "Employee not found." });

        var dto = _mapper.Map<EmployeeResponseDto>(employee);
        var pdfBytes = _pdfService.GenerateEmployeeCvPdf(dto);
        return File(pdfBytes, "application/pdf", $"CV_{employee.Name.Replace(" ", "_")}.pdf");
    }
}
