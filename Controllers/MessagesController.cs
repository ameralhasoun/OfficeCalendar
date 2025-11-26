using Microsoft.AspNetCore.Mvc;
using OfficeCalendar.Services;

namespace OfficeCalendar.Controllers;

[Route("api/v1/Message")]
public class MessagesController : Controller
{
    private readonly IMessageService _messageService;
    private readonly IEventsService _eventService;

    public MessagesController(IMessageService messageService, IEventsService eventService)
    {
        _messageService = messageService;
        _eventService = eventService;
    }

    private string? GetUserSessionKey()
        => HttpContext.Session.GetString("USER_SESSION_KEY");

    [HttpGet]
    public async Task<IActionResult> GetMessage()
    {
        int? userId = await _eventService.GetUserId(GetUserSessionKey());

        var messages = await _messageService.GetMessagesByUserId((int)userId);

        if (messages == null) return NoContent();
        return Ok(messages);
    }
}
