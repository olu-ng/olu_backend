

//// Models/ApplicationUser.cs
//using Microsoft.AspNetCore.Identity;
//using System;
//using System.Collections.Generic;

//namespace OluBackendApp.Models
//{
//    /// <summary>
//    /// Online/Offline/Away presence for chats.
//    /// </summary>
//    public enum PresenceStatus
//    {
//        Offline = 0,
//        Online = 1,
//        Away = 2
//    }

//    public class ApplicationUser : IdentityUser
//    {
//        // ─── PRESENCE ──────────────────────────────────────────────

//        /// <summary>Fingerprint of last verified device (for OTP, etc.).</summary>
//        public string? LastDeviceFingerprint { get; set; }

//        /// <summary>Current presence: online/offline/away.</summary>
//        public PresenceStatus CurrentStatus { get; set; }
//            = PresenceStatus.Offline;

//        /// <summary>Last time we saw them online.</summary>
//        public DateTime? LastSeenAt { get; set; }

//        // ─── PROFILES ──────────────────────────────────────────────

//        public ArtisanProfile? ArtisanProfile { get; set; }
//        public OfficeOwnerProfile? OfficeOwnerProfile { get; set; }
//        public AdminProfile? AdminProfile { get; set; }
//        public SuperAdminProfile? SuperAdminProfile { get; set; }

//        // ─── CHATS ────────────────────────────────────────────────

//        /// <summary>Chats started by this user.</summary>
//        public ICollection<Chat> ChatsInitiated { get; set; }
//            = new List<Chat>();

//        /// <summary>Chats where this user is the recipient.</summary>
//        public ICollection<Chat> ChatsReceived { get; set; }
//            = new List<Chat>();

//        // ─── MESSAGING ────────────────────────────────────────────

//        /// <summary>All messages this user has sent.</summary>
//        public ICollection<Message> MessagesSent { get; set; }
//            = new List<Message>();

//        /// <summary>All reactions this user has made.</summary>
//        public ICollection<Reaction> ReactionsMade { get; set; }
//            = new List<Reaction>();

//        // ─── BLOCKS ───────────────────────────────────────────────

//        /// <summary>Users this user has blocked.</summary>
//        public ICollection<Block> BlocksMade { get; set; }
//            = new List<Block>();

//        /// <summary>Users who have blocked this user.</summary>
//        public ICollection<Block> BlockedBy { get; set; }
//            = new List<Block>();

//    }
//}



// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace OluBackendApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>Fingerprint of last verified device (for OTP, etc.).</summary>
        public string? LastDeviceFingerprint { get; set; }

        /// <summary>Current presence: online/offline/away.</summary>
        public PresenceStatus CurrentStatus { get; set; }
            = PresenceStatus.Offline;

        /// <summary>Last time we saw them online.</summary>
        public DateTime? LastSeenAt { get; set; }

        // ─── PROFILES ──────────────────────────────────────────────

        public ArtisanProfile? ArtisanProfile { get; set; }
        public OfficeOwnerProfile? OfficeOwnerProfile { get; set; }
        public AdminProfile? AdminProfile { get; set; }
        public SuperAdminProfile? SuperAdminProfile { get; set; }

        // ─── CHATS ────────────────────────────────────────────────

        /// <summary>Chats started by this user.</summary>
        public ICollection<Chat> ChatsInitiated { get; set; }
            = new List<Chat>();

        /// <summary>Chats where this user is the recipient.</summary>
        public ICollection<Chat> ChatsReceived { get; set; }
            = new List<Chat>();

        // ─── MESSAGING ────────────────────────────────────────────

        /// <summary>All messages this user has sent.</summary>
        public ICollection<Message> MessagesSent { get; set; }
            = new List<Message>();

        /// <summary>All reactions this user has made.</summary>
        public ICollection<Reaction> ReactionsMade { get; set; }
            = new List<Reaction>();

        /// <summary>All threads this user has started (if you model that).</summary>
        public ICollection<ChatThread> ThreadsCreated { get; set; }
            = new List<ChatThread>();

        // ─── BLOCKS ───────────────────────────────────────────────

        /// <summary>Users this user has blocked.</summary>
        public ICollection<Block> BlocksMade { get; set; }
            = new List<Block>();

        /// <summary>Users who have blocked this user.</summary>
        public ICollection<Block> BlockedBy { get; set; }
            = new List<Block>();

         public ICollection<Block> BlocksInitiated { get; set; } = new List<Block>();
    public ICollection<Block> BlocksReceived { get; set; } = new List<Block>();
    }

    public enum PresenceStatus
    {
        Offline = 0,
        Online = 1,
        Away = 2
    }
}
