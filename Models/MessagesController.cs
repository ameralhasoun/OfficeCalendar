using Microsoft.AspNetCore.Mvc;
using OfficeCalendar.Models;
using OfficeCalendar.Services;
using Filter.UserRequired;

namespace OfficeCalendar.Models
{
    [UserRequired]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IEventsService _eventService;

        public MessageController(IMessageService messageService, IEventsService eventService)
        {
            _messageService = messageService;
            _eventService = eventService;
        }

        [HttpGet("api/v1/Message")]
        public async Task<IActionResult> GetMessage()
        {
            var userId = await _eventService.GetUserId(HttpContext.Session.GetString("USER_SESSION_KEY"));
            var messages = await _messageService.GetMessagesByUserId(userId);
            if (messages == null) return NoContent();
            return Ok(messages);
        }

        [HttpPost("api/v1/Message")]
        public async Task<IActionResult> PostMessage([FromQuery] int sendToUid, [FromBody] Message message)
        {
            var currentUid = await _eventService.GetUserId(HttpContext.Session.GetString("USER_SESSION_KEY"));
            if (currentUid == sendToUid) return BadRequest("Cannot send a message to yourself.");

            bool success = await _messageService.CreateMessage(message, sendToUid, currentUid);
            return success ? Ok("Message sent successfully.") : BadRequest("User not found.");
        }
    }
}
