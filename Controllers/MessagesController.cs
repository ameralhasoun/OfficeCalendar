using Microsoft.AspNetCore.Mvc;
using OfficeCalendar.Models;
using OfficeCalendar.Services;
using Filter.AdminRequired;
using Filter.UserRequired;
using Microsoft.EntityFrameworkCore;

namespace OfficeCalendar.Controllers;

[Route("api/v1/Message")]
public class MessagesController : Controller
{
    private readonly IMessageService _messageService;
    private readonly IEventsService _eventService;
    private readonly DatabaseContext _context;

    public MessagesController(IMessageService messageService, IEventsService eventService, DatabaseContext context)
    {
        _messageService = messageService;
        _eventService = eventService;
        _context = context;
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
        return Ok(messages);
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

        message.FromUserId = currentUid;
        message.ToUserId = sendToUid;
        message.Date = DateTime.Now;
        message.BeenRead = false;

        _context.Message.Add(message);
        await _context.SaveChangesAsync();

        return Ok("Message sent directly through DbContext.");
    }
}
