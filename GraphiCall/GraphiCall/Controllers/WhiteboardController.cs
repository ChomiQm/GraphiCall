using Microsoft.AspNetCore.Mvc;
using GraphiCall.Data;
using Microsoft.EntityFrameworkCore;
using GraphiCall.Client.DTO;

[Route("whiteboards")]
[ApiController]
public class WhiteboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WhiteboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("{userId}/addWhiteboard")]
    public async Task<ActionResult<WhiteboardDto>> PostWhiteboard([FromRoute] string userId, [FromBody] WhiteboardDto whiteboardDto)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Sprawdź, czy użytkownik już posiada whiteboard
        var existingWhiteboard = await _context.Whiteboards
                                               .FirstOrDefaultAsync(wb => wb.ApplicationUserId == userId);
        if (existingWhiteboard != null)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        var whiteboard = new Whiteboard()
        {
            WhiteboardId = Guid.NewGuid().ToString(), // Generuj nowe ID
            Data = whiteboardDto.Data,
            ApplicationUserId = userId // Użyj userId z parametru
        };

        _context.Whiteboards.Add(whiteboard);
        try
        {
            await _context.SaveChangesAsync();
            var createdWhiteboardDto = ConvertToWhiteboardDto(whiteboard);
            return CreatedAtAction(nameof(GetWhiteboard), new { userId = userId }, createdWhiteboardDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{userId}/getWhiteboard")]
    public async Task<ActionResult<Whiteboard>> GetWhiteboard([FromRoute] string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var whiteboard = await _context.Whiteboards
                                       .FirstOrDefaultAsync(wb => wb.ApplicationUserId == userId);

        if (whiteboard == null)
        {
            return NotFound();
        }

        return whiteboard;
    }

    [HttpPut("{userId}/updateWhiteboard/{id}")]
    public async Task<IActionResult> PutWhiteboard([FromRoute] string userId, [FromRoute] string id, [FromBody] Whiteboard whiteboard)
    {
        if (id != whiteboard.WhiteboardId)
        {
            return BadRequest();
        }

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        _context.Entry(whiteboard).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!WhiteboardExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // HELPERS 

    [HttpGet("{userId}/getWhiteboardId")]
    public async Task<ActionResult<string>> GetWhiteboardId([FromRoute] string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var whiteboard = await _context.Whiteboards
                                       .Where(w => w.ApplicationUserId == userId)
                                       .Select(w => w.WhiteboardId)
                                       .FirstOrDefaultAsync();

        if (whiteboard == null)
        {
            return NotFound();
        }

        return Ok(whiteboard);
    }

    [HttpGet("{userId}/checkWhiteboardExists")]
    public async Task<ActionResult<bool>> CheckWhiteboardExists([FromRoute] string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var whiteboardExists = await _context.Whiteboards
                                             .AnyAsync(w => w.ApplicationUserId == userId);

        return Ok(whiteboardExists);
    }

    private WhiteboardDto ConvertToWhiteboardDto(Whiteboard whiteboard)
    {
        return new WhiteboardDto
        {
            WhiteboardId = whiteboard.WhiteboardId,
            Data = whiteboard.Data,
            ApplicationUserId = whiteboard.ApplicationUserId
        };
    }


    private bool WhiteboardExists(string id)
    {
        return _context.Whiteboards.Any(e => e.WhiteboardId == id);
    }
}
