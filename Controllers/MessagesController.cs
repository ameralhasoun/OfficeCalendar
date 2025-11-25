using Microsoft.AspNetCore.Mvc;
using OfficeCalendar.Models;
using OfficeCalendar.Services;
using Filter.UserRequired;

namespace OfficeCalendar.Controllers;

[Route("api/v1/MessageScenario7a")]
public class MessagesControllerScenario7a : Controller
{
    private readonly IMessageService _messageService;
    private readonly IEventsService _eventService;

    public MessagesControllerScenario7a(IMessageService messageService, IEventsService eventService)
    {
        _messageService = messageService;
        _eventService = eventService;
    }

    private string? GetUserSessionKey()
        => HttpContext.Session.GetString("USER_SESSION_KEY");

    [UserRequired]
    [HttpGet]
    public async Task<IActionResult> GetMessage()
    {
        int? userId = await _eventService.GetUserId(GetUserSessionKey());
        if (userId == null) return BadRequest("User not found.");

        List<Message>? messages = null;

        int totalMessages = messages.Count;
        int readMessages = messages.Count(m => m.BeenRead);

        return Ok(new
        {
            total = totalMessages,
            read = readMessages,
            ratio = 0
        });
    }
}
