using Microsoft.AspNetCore.Mvc;
using OfficeCalendar.Services;
namespace OfficeCalendar.Controllers;

[Route("api/v1/Messages")]
public class MessagesController : Controller
{
    private readonly IMessageService _messageService;
    private readonly IEventsService _eventService;

    public MessagesController(IMessageService messageService, IEventsService eventService   )
    {
        _messageService = messageService;
        _eventService = eventService;
    }

    [HttpGet("UserMessages")]
    public async Task<IActionResult> GetUserMessages()
    {
        string sessionKey = HttpContext.Session.GetString("USER_SESSION_KEY")!;
        var userId = await _eventService.GetUserId(sessionKey);

        var messages = await _messageService.GetMessagesByUserId(userId);
    
        if (messages == null || messages.Count == 0)
        {
            return NoContent();
        }

        return Ok(messages);
    }
}
