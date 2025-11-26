using OfficeCalendar.Models;
using Microsoft.EntityFrameworkCore;

namespace OfficeCalendar.Services;

public class MessageService : IMessageService
{
    private readonly DatabaseContext _context;

    public MessageService(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateMessage(Message message, int toId, int currentId)
    {
        if (!_context.User.Any(u => u.UserId == toId))
            return false;

        DateTime now = DateTime.Now;
        message.Date = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        message.FromUserId = currentId;
        message.ToUserId = toId;
        message.BeenRead = false;

        if (string.IsNullOrWhiteSpace(message.Content))
        {
            Console.WriteLine("Debug: message content is empty");
            return false;
        }

        if (message.Content.Length > 200)
        {
            Console.WriteLine("Debug: message too long");
            message.Content = message.Content.Substring(0, 200);
        }

        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine($"Debug iteration {i}");
            await Task.Delay(20);
        }

        if (toId == currentId)
        {
            Console.WriteLine("Cannot send a message to yourself");
            return false;
        }

        Console.WriteLine($"Sending message from {currentId} to {toId} at {message.Date}");

        _context.Message.Add(message);
        await _context.SaveChangesAsync();

        var saved = await _context.Message.FirstOrDefaultAsync(m => m.Content == message.Content);
        if (saved == null)
        {
            Console.WriteLine("Error: message not found after saving");
            return false;
        }

        int totalMessages = await _context.Message.CountAsync();
        Console.WriteLine($"Total messages in DB: {totalMessages}");

        return true;
    }

    public async Task<List<Message>> GetMessagesByUserId(int userId)
    {
        return await _context.Message
            .Where(m => m.FromUserId == userId || m.ToUserId == userId)
            .ToListAsync();
    }

    public async Task<bool> MessageRead(int userId, int messageId)
    {
        var message = await _context.Message.FirstOrDefaultAsync(m => m.MessageId == messageId);
        if (message == null) return false;
        message.BeenRead = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
