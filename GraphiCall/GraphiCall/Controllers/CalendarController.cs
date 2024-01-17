using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraphiCall.Data;
using GraphiCall.Client.DTO;

[Route("calendars")]
[ApiController]
public class CalendarController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CalendarController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Currently usables

    [HttpGet("getCalendar/{userId}/with-events")]
    public async Task<ActionResult<Calendar>> GetCalendarWithEvents([FromRoute] string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var calendar = await _context.Calendars
                                     .Include(c => c.Events)
                                     .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

        if (calendar == null)
        {
            return NotFound();
        }

        return calendar;
    }

    [HttpPost("addCalendar/{userId}")]
    public async Task<ActionResult<CalendarDto>> CreateCalendar([FromRoute] string userId, [FromBody] CalendarDto calendarDto)
    {
        if (userId == null)
        {
            return Unauthorized();
        }

        var calendar = new Calendar
        {
            CalendarId = calendarDto.CalendarId,
            Name = calendarDto.Name,
            Description = calendarDto.Description,
            ApplicationUserId = calendarDto.ApplicationUserId
        };

        _context.Calendars.Add(calendar);
        await _context.SaveChangesAsync();

        var createdCalendarDto = ConvertToCalendarDto(calendar);

        return CreatedAtAction(nameof(GetCalendar), new { userId = userId, calendarId = calendar.CalendarId }, createdCalendarDto);
    }

    [HttpPut("{userId}/updateCalendar/{calendarId}")]
    public async Task<IActionResult> UpdateCalendar([FromRoute] string userId, [FromRoute] string calendarId, [FromBody] CalendarDto calendarDto)
    {
        if (userId == null)
        {
            return Unauthorized();
        }

        var calendar = await _context.Calendars
                                     .FirstOrDefaultAsync(c => c.CalendarId == calendarId && c.ApplicationUserId == userId);

        if (calendar == null)
        {
            return NotFound();
        }

        calendar.Name = calendarDto.Name;
        calendar.Description = calendarDto.Description;
        // Nie aktualizuj Events ani ApplicationUserId

        _context.Entry(calendar).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Calendars.Any(c => c.CalendarId == calendarId && c.ApplicationUserId == userId))
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

    [HttpDelete("{userId}/deleteCalendar/{calendarId}")]
    public async Task<IActionResult> DeleteCalendar([FromRoute] string userId, [FromRoute] string calendarId)
    {
        if (userId == null)
        {
            return Unauthorized();
        }

        var calendar = await _context.Calendars
                                     .FirstOrDefaultAsync(c => c.CalendarId == calendarId && c.ApplicationUserId == userId);

        if (calendar == null)
        {
            return NotFound();
        }

        _context.Calendars.Remove(calendar);
        await _context.SaveChangesAsync();

        return NoContent();
    }


    // HELPERS

    [HttpGet("{userId}")]
    public async Task<ActionResult<Calendar>> GetCalendar([FromRoute] string userId)
    {

        if (userId == null)
        {
            return Unauthorized();
        }

        var calendar = await _context.Calendars
                                    .Include(c => c.Events)
                                    .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

        if (calendar == null)
        {
            return NotFound();
        }

        return calendar;
    }

    [HttpGet("getCalendarId/{userId}")]
    public async Task<ActionResult<string>> GetCalendarId([FromRoute] string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var calendar = await _context.Calendars
                                    .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

        if (calendar == null)
        {
            return NotFound();
        }

        return Ok(calendar.CalendarId);
    }


    [HttpGet("checkCalendar/{userId}")]
    public async Task<ActionResult<bool>> CheckIfCalendarExists([FromRoute] string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var calendarExists = await _context.Calendars
                                           .AnyAsync(c => c.ApplicationUserId == userId);


        return calendarExists;
    }


    // ---------------------------EVENTS
    // Currently usables

    [HttpGet("getCalendar/{userId}/getEvents/{calendarId}")]
    public async Task<ActionResult<IEnumerable<CalendarEvent>>> GetAllEvents([FromRoute] string userId, [FromRoute] string calendarId)
    {

        if (userId == null)
        {
            return Unauthorized();
        }

        var events = await _context.CalendarEvents
                                   .Where(e => e.FK_CalendarId == calendarId)
                                   .ToListAsync();

        if (events == null)
        {
            return NotFound();
        }

        return Ok(events);
    }

    [HttpPost("{userId}/addEventToCalendar/{calendarId}")]
    public async Task<ActionResult<CalendarEventDto>> AddEventToCalendar([FromRoute] string userId, [FromRoute] string calendarId, [FromBody] CalendarEventDto calendarEventDto)
    {
        if (userId == null)
        {
            return Unauthorized();
        }

        var calendarEvent = ConvertToCalendarEvent(calendarEventDto);
        _context.CalendarEvents.Add(calendarEvent);
        await _context.SaveChangesAsync();

        // Assuming you have a way to get the newly added event with its ID populated, e.g.:
        var addedEvent = await _context.CalendarEvents.FindAsync(calendarEvent.CalendarEventId);

        return CreatedAtAction(
     nameof(GetEvent),
     new { userId = userId, calendarId = calendarId, eventId = addedEvent.CalendarEventId },
     addedEvent);

    }

    [HttpDelete("{userId}/deleteEventFromCalendar/{calendarId}/events/{eventId}")]
    public async Task<IActionResult> DeleteEventFromCalendar([FromRoute] string userId, [FromRoute] string calendarId, [FromRoute] string eventId)
    {

        if (userId == null)
        {
            return Unauthorized();
        }

        var calendarEvent = await _context.CalendarEvents
                                          .FirstOrDefaultAsync(ce => ce.CalendarEventId == eventId && ce.FK_CalendarId == calendarId);
        if (calendarEvent == null)
        {
            return NotFound();
        }

        _context.CalendarEvents.Remove(calendarEvent);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{userId}/updateEventInCalendar/{calendarId}/events/{eventId}")]
    public async Task<IActionResult> UpdateCalendarEvent([FromRoute] string userId, [FromRoute] string calendarId, [FromRoute] string eventId, [FromBody] CalendarEvent calendarEvent)
    {

        if (userId == null)
        {
            return Unauthorized();
        }

        if (eventId != calendarEvent.CalendarEventId || calendarEvent.FK_CalendarId != calendarId)
        {
            return BadRequest();
        }

        _context.Entry(calendarEvent).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.CalendarEvents.Any(e => e.CalendarEventId == eventId && e.FK_CalendarId == calendarId))
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

    //HELPERS

    [HttpGet("getCalendar/{userId}/getEvent/{calendarId}")]
    public async Task<ActionResult<CalendarEvent>> GetEvent([FromRoute] string userId, [FromRoute] string calendarId, [FromRoute] string eventId)
    {

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(calendarId))
        {
            return NotFound();
        }

        var calendarEvent = await _context.CalendarEvents
                                          .FirstOrDefaultAsync(e => e.CalendarEventId == eventId && e.FK_CalendarId == calendarId);

        if (calendarEvent == null)
        {
            return NotFound();
        }

        return Ok(calendarEvent);
    }

    private CalendarDto ConvertToCalendarDto(Calendar calendar)
    {
        return new CalendarDto
        {
            CalendarId = calendar.CalendarId,
            Name = calendar.Name,
            Description = calendar.Description,
            Events = calendar.Events != null ? ConvertToCalendarEventDtos(calendar.Events) : null,
            ApplicationUserId = calendar.ApplicationUserId
        };
    }

    private List<CalendarEventDto> ConvertToCalendarEventDtos(List<CalendarEvent> events)
    {
        return events.Select(e => new CalendarEventDto
        {
            CalendarEventId = e.CalendarEventId,
            FK_CalendarId = e.FK_CalendarId,
            Title = e.Title,
            EventDate = e.EventDate,
            FromDate = e.FromDate,
            ToDate = e.ToDate,
            DateValue = e.DateValue,
            DayName = e.DayName,
            Color = e.Color,
            Description = e.Description
        }).ToList();
    }

    private CalendarEvent ConvertToCalendarEvent(CalendarEventDto dto)
    {
        return new CalendarEvent
        {

            CalendarEventId = dto.CalendarEventId,
            FK_CalendarId = dto.FK_CalendarId,
            Title = dto.Title,
            EventDate = dto.EventDate,
            FromDate = dto.FromDate,
            ToDate = dto.ToDate,
            DateValue = dto.DateValue,
            DayName = dto.DayName,
            Color = dto.Color,
            Description = dto.Description
        };
    }

}
