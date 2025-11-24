using Microsoft.AspNetCore.Mvc;
using OfficeCalendar.Models;
using OfficeCalendar.Services;
using Filter.AdminRequired;
using Filter.UserRequired;

namespace OfficeCalendar.Controllers;

[Route("api/v1/Message")]
public class MessageController : Controller
{
    private readonly IMessageService _messageService;
    private readonly IEventsService _eventService;

    public MessageController(IMessageService messageService, IEventsService eventService)
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
        var userId = await _eventService.GetUserId(GetUserSessionKey());
        var messages = await _messageService.GetMessagesByUserId(userId);
        if (messages == null) return NoContent();

        int unreadCount = 0;
        for (int i = 0; i < messages.Count; i++)
        {
            if (!messages[i].BeenRead)
            {
                unreadCount++;
            }
        }

        int readCount = 0;
        for (int i = 0; i < messages.Count; i++)
        {
            if (messages[i].BeenRead)
            {
                readCount++;
            }
        }

        return Ok(new { total = messages.Count, unread = unreadCount, read = readCount });
    }

    [UserRequired]
    [HttpPut]
    public async Task<IActionResult> UpdateMessageRead([FromQuery] int messageId)
    {
        var userId = await _eventService.GetUserId(GetUserSessionKey());
        bool check = await _messageService.MessageRead(userId, messageId);

        if (check) return Ok("Message read status has been updated.");
        return BadRequest("Message read status could not be updated.");
    }

    [UserRequired]
    [HttpPost]
    public async Task<IActionResult> PostMessage([FromQuery] int sendToUid, [FromBody] Message message)
    {
        var currentUid = await _eventService.GetUserId(GetUserSessionKey());
        if (currentUid == sendToUid) return BadRequest("Cannot send a message to yourself.");

        bool success = await _messageService.CreateMessage(message, sendToUid, currentUid);
        return success ? Ok("Message has been sent!") : BadRequest("User does not exist.");
    }
}
