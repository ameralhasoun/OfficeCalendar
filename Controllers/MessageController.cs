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

    [UserRequired]
    [HttpGet]
    public async Task<IActionResult> GetMessage()
    {
        var userId = await _eventService.GetUserId(HttpContext.Session.GetString("USER_SESSION_KEY")); // herhaling 1
        var messages = await _messageService.GetMessagesByUserId(userId);
        if (messages == null) return NoContent();
        return Ok(messages);
    }

    [UserRequired]
    [HttpPut]
    public async Task<IActionResult> UpdateMessageRead([FromQuery] int messageId)
    {
        var userId = await _eventService.GetUserId(HttpContext.Session.GetString("USER_SESSION_KEY")); // herhaling 2
        bool check = await _messageService.MessageRead(userId, messageId);

        if (check) return Ok("Message read status has been updated.");
        return BadRequest("Message read status could not be updated.");
    }

    [UserRequired]
    [HttpPost]
    public async Task<IActionResult> PostMessage([FromQuery] int sendToUid, [FromBody] Message message)
    {
        var currentUid = await _eventService.GetUserId(HttpContext.Session.GetString("USER_SESSION_KEY")); // herhaling 3
        if (currentUid == sendToUid) return BadRequest("Cannot send a message to yourself.");

        bool success = await _messageService.CreateMessage(message, sendToUid, currentUid);
        return success ? Ok("Message has been sent!") : BadRequest("User does not exist.");
    }
}
