using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraphiCall.Data;
using GraphiCall.Client.DTO;

[Route("notes")]
[ApiController]
public class NotesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}/getAllNotes")]
    public async Task<ActionResult<IEnumerable<Note>>> GetAllNotes([FromRoute] string userId)
    {
        var notes = await _context.Notes
                                  .Where(n => n.ApplicationUserId == userId)
                                  .ToListAsync();

        return notes;
    }

    [HttpGet("{userId}/getNote/{noteId}")] // Poprawiono trasę o brakujące {noteId}
    public async Task<ActionResult<Note>> GetNoteById([FromRoute] string userId, [FromRoute] string noteId)
    {
        var note = await _context.Notes
                                 .FirstOrDefaultAsync(n => n.NoteId == noteId && n.ApplicationUserId == userId);

        if (note == null)
        {
            return NotFound();
        }

        return note;
    }

    [HttpPost("{userId}/createNote")]
    public async Task<ActionResult<Note>> CreateNote([FromRoute] string userId, [FromBody] NoteDto noteDto)
    {
        // Tworzenie nowej notatki z danych DTO
        var note = new Note
        {
            NoteId = Guid.NewGuid().ToString(),
            Title = noteDto.Title,
            Content = noteDto.Content,
            CreatedAt = DateTime.UtcNow, // Ustaw bieżący czas UTC jako czas utworzenia
            UpdatedAt = noteDto.UpdatedAt,
            Priority = (GraphiCall.Data.NotePriority)noteDto.Priority,
            ApplicationUserId = userId // Ustaw użytkownika na podstawie userId z trasy
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        // Zwraca lokalizację nowo utworzonej notatki
        return CreatedAtAction(nameof(GetNoteById), new { userId = userId, noteId = note.NoteId }, note);
    }

    [HttpPut("{userId}/updateNote/{noteId}")]
    public async Task<IActionResult> UpdateNote([FromRoute] string userId, [FromRoute] string noteId, [FromBody] Note note)
    {
        if (noteId != note.NoteId)
        {
            return BadRequest();
        }

        if (userId != note.ApplicationUserId)
        {
            return Unauthorized();
        }

        _context.Entry(note).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Notes.Any(n => n.NoteId == noteId))
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

    [HttpDelete("{userId}/deleteNote/{noteId}")]
    public async Task<IActionResult> DeleteNote([FromRoute] string userId, [FromRoute] string noteId)
    {
        var note = await _context.Notes
                                 .FirstOrDefaultAsync(n => n.NoteId == noteId && n.ApplicationUserId == userId);

        if (note == null)
        {
            return NotFound();
        }

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
