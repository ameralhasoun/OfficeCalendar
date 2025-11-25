using Microsoft.AspNetCore.Mvc;
using OfficeCalendar.Models;
using OfficeCalendar.Services;
using Filter.AdminRequired;
using Filter.UserRequired;

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

    [UserRequired]
    [HttpGet]
    public async Task<IActionResult> GetMessage()
    {
        int? userId = await _eventService.GetUserId(GetUserSessionKey());
        if (userId == null) return BadRequest("User not found.");
        var messages = await _messageService.GetMessagesByUserId((int)userId);

        int totalMessages = messages.Count;
        int readMessages = messages.Count(m => m.BeenRead);

        double readRatio = totalMessages == 0 ? 0 : (double)readMessages / totalMessages;

        return Ok(new
        {
            total = totalMessages,
            read = readMessages,
            ratio = readRatio
        });
    }

    [UserRequired]
    [HttpPut]
    public async Task<IActionResult> UpdateMessageRead([FromQuery] int messageId)
    {
        int? userId = await _eventService.GetUserId(GetUserSessionKey());
        if (userId == null) return BadRequest("User not found.");
        bool check = await _messageService.MessageRead((int)userId, messageId);

        if (check) return Ok("Message read status has been updated.");
        return BadRequest("Message read status could not be updated.");
    }

    [UserRequired]
    [HttpPost]
    public async Task<IActionResult> PostMessage([FromQuery] int sendToUid, [FromBody] Message message)
    {
        int? currentUid = await _eventService.GetUserId(GetUserSessionKey());
        if (currentUid == null) return BadRequest("User not found.");
        if (currentUid == sendToUid) return BadRequest("Cannot send a message to yourself.");

        bool success = await _messageService.CreateMessage(message, sendToUid, (int)currentUid);
        return success ? Ok("Message has been sent!") : BadRequest("User does not exist.");
    }
}
