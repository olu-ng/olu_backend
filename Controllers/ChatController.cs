using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OluBackendApp.Data;
using OluBackendApp.DTOs;
using OluBackendApp.Models;

namespace OluBackendApp.Controllers
{
    [ApiController]
    [Route("api/chats")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public ChatController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }

        // 1) OfficeOwner starts a chat
        [HttpPost]
        [Authorize(Roles = Roles.OfficeOwner)]
        public async Task<ActionResult<ChatSummaryDto>> Create([FromBody] CreateChatDto dto)
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var artisan = await _um.FindByIdAsync(dto.RecipientId);
            if (artisan == null || !await _um.IsInRoleAsync(artisan, Roles.Artisan))
                return BadRequest("Invalid recipient.");

            if (await _db.Chats.AnyAsync(c =>
                c.InitiatorId == me.Id && c.RecipientId == dto.RecipientId))
                return Conflict("Chat already exists.");

            // refuse if recipient has blocked me
            if (await _db.Blocks.AnyAsync(b =>
                    b.BlockerId == dto.RecipientId && b.BlockedId == me.Id))
                return Forbid("You are blocked by this user.");

            var chat = new Chat
            {
                InitiatorId = me.Id,
                RecipientId = dto.RecipientId,
                CreatedAt = DateTime.UtcNow
            };
            _db.Chats.Add(chat);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = chat.Id },
                new ChatSummaryDto(
                    chat.Id,
                    chat.InitiatorId,
                    chat.RecipientId,
                    chat.CreatedAt,
                    null,
                    null
                ));
        }

        // 2) List all chats for current user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatSummaryDto>>> GetAll()
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var list = await _db.Chats
                .AsNoTracking()
                .Where(c => c.InitiatorId == me.Id || c.RecipientId == me.Id)
                .Select(c => new ChatSummaryDto(
                    c.Id,
                    c.InitiatorId,
                    c.RecipientId,
                    c.CreatedAt,
                    c.Messages.OrderByDescending(m => m.SentAt).Select(m => m.Content).FirstOrDefault(),
                    c.Messages.OrderByDescending(m => m.SentAt).Select(m => (DateTime?)m.SentAt).FirstOrDefault()
                ))
                .ToListAsync();

            return Ok(list);
        }

        // 3) Get a single chat
        [HttpGet("{id}")]
        public async Task<ActionResult<ChatSummaryDto>> GetById(int id)
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var chat = await _db.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (chat == null) return NotFound();
            if (chat.InitiatorId != me.Id && chat.RecipientId != me.Id)
                return Forbid();

            var last = chat.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
            return new ChatSummaryDto(
                chat.Id,
                chat.InitiatorId,
                chat.RecipientId,
                chat.CreatedAt,
                last?.Content,
                last?.SentAt
            );
        }

        // 4) Read messages (mark Sent→Delivered)
        [HttpGet("{chatId}/messages")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(int chatId)
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var chat = await _db.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat == null) return NotFound();
            if (chat.InitiatorId != me.Id && chat.RecipientId != me.Id)
                return Forbid();

            // auto‑deliver
            var toDeliver = chat.Messages
                .Where(m => m.Status == MessageStatus.Sent && m.SenderId != me.Id)
                .ToList();
            if (toDeliver.Count > 0)
            {
                toDeliver.ForEach(m =>
                {
                    m.Status = MessageStatus.Delivered;
                    m.DeliveredAt = DateTime.UtcNow;
                });
                await _db.SaveChangesAsync();
            }

            var dtos = chat.Messages
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto(
                    m.Id,
                    m.SenderId,
                    Markdown.ToHtml(m.Content),
                    m.SentAt,
                    (OluBackendApp.DTOs.MessageStatus)m.Status,
                    m.DeliveredAt,
                    m.ReadAt,
                    m.IsEdited,
                    m.EditedAt,
                    m.IsDeleted
                ))
                .ToList();

            return Ok(dtos);
        }

        // 5) Send a new message
        [HttpPost("{chatId}/messages")]
        public async Task<ActionResult<MessageDto>> Send(int chatId, [FromBody] CreateMessageDto dto)
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var chat = await _db.Chats.FindAsync(chatId);
            if (chat == null) return NotFound();
            if (chat.InitiatorId != me.Id && chat.RecipientId != me.Id)
                return Forbid();

            var msg = new Message
            {
                ChatId = chatId,
                SenderId = me.Id,
                Content = dto.Content,
                SentAt = DateTime.UtcNow,
                Status = MessageStatus.Sent
            };
            _db.Messages.Add(msg);

            _db.AuditLogs.Add(new AuditLog
            {
                Entity = "Message",
                Action = "Create",
                UserId = me.Id,
                Details = $"Len={dto.Content.Length}",
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessages), new { chatId },
                new MessageDto(
                    msg.Id,
                    msg.SenderId,
                    Markdown.ToHtml(msg.Content),
                    msg.SentAt,
                    (OluBackendApp.DTOs.MessageStatus)msg.Status,
                    msg.DeliveredAt,
                    msg.ReadAt,
                    msg.IsEdited,
                    msg.EditedAt,
                    msg.IsDeleted
                ));
        }

        // 6) Edit message
        [HttpPut("messages/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] UpdateMessageDto dto)
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var msg = await _db.Messages.FindAsync(id);
            if (msg == null) return NotFound();
            if (msg.SenderId != me.Id) return Forbid();

            msg.Content = dto.NewContent;
            msg.IsEdited = true;
            msg.EditedAt = DateTime.UtcNow;

            _db.AuditLogs.Add(new AuditLog
            {
                Entity = "Message",
                Action = "Edit",
                UserId = me.Id,
                Details = $"MsgId={id}",
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 7) Soft‑delete message
        [HttpDelete("messages/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var msg = await _db.Messages.FindAsync(id);
            if (msg == null) return NotFound();
            if (msg.SenderId != me.Id) return Forbid();

            msg.IsDeleted = true;
            _db.AuditLogs.Add(new AuditLog
            {
                Entity = "Message",
                Action = "Delete",
                UserId = me.Id,
                Details = $"MsgId={id}",
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 8) React to a message
        [HttpPost("messages/{id}/reactions")]
        public async Task<IActionResult> React(int id, [FromBody] ReactionDto dto)
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var msg = await _db.Messages.FindAsync(id);
            if (msg == null) return NotFound();

            var reaction = new Reaction
            {
                MessageId = id,
                UserId = me.Id,
                Emoji = dto.Emoji
            };
            _db.Reactions.Add(reaction);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessages),
                new { chatId = msg.ChatId },
                new ReactionDto(id, me.Id, dto.Emoji));
        }

        // 9) Threads
        [HttpPost("messages/{id}/threads")]
        public async Task<ActionResult<ThreadDto>> CreateThread(int id)
        {
            var msg = await _db.Messages.FindAsync(id);
            if (msg == null) return NotFound();

            var thread = new ChatThread { ParentMsgId = id };
            _db.ChatThreads.Add(thread);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessages),
                new { chatId = msg.ChatId },
                new ThreadDto(thread.Id, id, Array.Empty<MessageDto>()));
        }

        [HttpGet("messages/{id}/threads")]
        public async Task<ActionResult<ThreadDto>> GetThread(int id)
        {
            var thread = await _db.ChatThreads
                .Include(t => t.Replies)
                .FirstOrDefaultAsync(t => t.ParentMsgId == id);
            if (thread == null) return NotFound();

            var replies = thread.Replies.Select(m => new MessageDto(
                m.Id,
                m.SenderId,
                Markdown.ToHtml(m.Content),
                m.SentAt,
                (OluBackendApp.DTOs.MessageStatus)m.Status,
                m.DeliveredAt,
                m.ReadAt,
                m.IsEdited,
                m.EditedAt,
                m.IsDeleted
            ));
            return Ok(new ThreadDto(thread.Id, id, replies));
        }

        // 10) Unread counts
        [HttpGet("unread/count")]
        public async Task<ActionResult<int>> GetGlobalUnread()
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var count = await _db.Messages
                .Where(m => m.Chat.RecipientId == me.Id && m.Status != MessageStatus.Read)
                .CountAsync();
            return Ok(count);
        }

        [HttpGet("unread")]
        public async Task<ActionResult<IEnumerable<object>>> GetUnreadByChat()
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var data = await _db.Chats
                .Where(c => c.InitiatorId == me.Id || c.RecipientId == me.Id)
                .Select(c => new {
                    ChatId = c.Id,
                    UnreadCount = c.Messages
                        .Count(m => m.SenderId != me.Id && m.Status != MessageStatus.Read)
                })
                .ToListAsync();
            return Ok(data);
        }

        // 11) Soft‑delete a chat
        [HttpDelete("{chatId}")]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var chat = await _db.Chats.FindAsync(chatId);
            if (chat == null) return NotFound();
            if (chat.InitiatorId != me.Id && chat.RecipientId != me.Id)
                return Forbid();

            chat.IsDeleted = true;
            chat.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 12) Block a user
        [HttpPost("block")]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserDto dto)
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();
            if (dto.UserId == me.Id) return BadRequest("Cannot block yourself.");

            if (await _db.Blocks.AnyAsync(b => b.BlockerId == me.Id && b.BlockedId == dto.UserId))
                return Conflict("Already blocked.");

            var block = new Block
            {
                BlockerId = me.Id,
                BlockedId = dto.UserId,
                CreatedAt = DateTime.UtcNow
            };
            _db.Blocks.Add(block);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 13) Unblock a user
        [HttpDelete("block/{userId}")]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            var me = await _um.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var block = await _db.Blocks
                .FirstOrDefaultAsync(b => b.BlockerId == me.Id && b.BlockedId == userId);
            if (block == null) return NotFound();

            _db.Blocks.Remove(block);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
