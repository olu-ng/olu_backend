using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OluBackendApp.Data;
using OluBackendApp.DTOs;
using OluBackendApp.Models;
using static OluBackendApp.Models.ApplicationUser;

namespace OluBackendApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public ChatHub(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _um.GetUserAsync(Context.User!);
            if (user != null)
            {
                user.CurrentStatus = PresenceStatus.Online;
                user.LastSeenAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                await Clients.Others.SendAsync("UserPresenceChanged",
                    user.Id, user.CurrentStatus, user.LastSeenAt, null);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var user = await _um.GetUserAsync(Context.User!);
            if (user != null)
            {
                user.CurrentStatus = PresenceStatus.Offline;
                user.LastSeenAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                await Clients.Others.SendAsync("UserPresenceChanged",
                    user.Id, user.CurrentStatus, user.LastSeenAt, null);
            }
            await base.OnDisconnectedAsync(ex);
        }

        public Task JoinChat(int chatId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, $"chat-{chatId}");

        public Task LeaveChat(int chatId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat-{chatId}");

        public async Task SendTyping(int chatId, bool isTyping)
        {
            var userId = Context.UserIdentifier;
            await Clients.GroupExcept($"chat-{chatId}", Context.ConnectionId)
                         .SendAsync("UserTyping", chatId, userId, isTyping);
        }

        public async Task SetAway(string? statusMessage = null)
        {
            var user = await _um.GetUserAsync(Context.User!);
            if (user != null)
            {
                user.CurrentStatus = PresenceStatus.Away;
                user.LastSeenAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                await Clients.Others.SendAsync("UserPresenceChanged",
                    user.Id, user.CurrentStatus, user.LastSeenAt, statusMessage);
            }
        }
    }
}
