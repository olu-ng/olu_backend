//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;
//using System.Text.Json;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.ChangeTracking;
//using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
//using OluBackendApp.Models;

//namespace OluBackendApp.Data
//{
//    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
//    {
//        public DbSet<ArtisanProfile> ArtisanProfiles { get; set; }
//        public DbSet<OfficeOwnerProfile> OfficeOwnerProfiles { get; set; }
//        public DbSet<AdminProfile> AdminProfiles { get; set; }
//        public DbSet<SuperAdminProfile> SuperAdminProfiles { get; set; }
//        public DbSet<OtpRecord> OtpRecords { get; set; }

//        // Chat support
//        public DbSet<Chat> Chats { get; set; }
//        public DbSet<Message> Messages { get; set; }
//        public DbSet<Reaction> Reactions { get; set; }
//        public DbSet<ChatThread> ChatThreads { get; set; }
//        public DbSet<Block> Blocks { get; set; }
//        public DbSet<Attachment> Attachments { get; set; }
//        public DbSet<AuditLog> AuditLogs { get; set; }
//        public DbSet<PushSubscription> PushSubscriptions { get; set; }

//        public DbSet<JobPost> JobPosts { get; set; }

//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//            : base(options) { }

//        protected override void OnModelCreating(ModelBuilder builder)
//        {
//            base.OnModelCreating(builder);

//            // —— JSON-LIST CONVERTERS —— 
//            var jsonConverter = new ValueConverter<List<string>?, string?>(
//                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
//                v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
//            );

//            var listComparer = new ValueComparer<List<string>?>( 
//                (c1, c2) => c1 == null && c2 == null || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
//                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v != null ? v.GetHashCode() : 0)),
//                c => c == null ? null : new List<string>(c)
//            );

//            // —— PROFILE CONFIGS —— 
//            builder.Entity<ArtisanProfile>(b =>
//            {
//                b.HasKey(p => p.UserId);
//                b.HasOne(p => p.User)
//                    .WithOne(u => u.ArtisanProfile)
//                    .HasForeignKey<ArtisanProfile>(p => p.UserId);

//                b.Property(p => p.PhoneNumbers)
//                    .HasConversion(jsonConverter)
//                    .HasColumnType("NVARCHAR(MAX)")
//                    .Metadata.SetValueComparer(listComparer);

//                b.Property(p => p.ServicesOffered)
//                    .HasConversion(jsonConverter)
//                    .HasColumnType("NVARCHAR(MAX)")
//                    .Metadata.SetValueComparer(listComparer);

//                b.Property(p => p.ProfessionTags)
//                    .HasConversion(jsonConverter)
//                    .HasColumnType("NVARCHAR(MAX)")
//                    .Metadata.SetValueComparer(listComparer);
//            });

//            builder.Entity<OfficeOwnerProfile>(b =>
//            {
//                b.HasKey(p => p.UserId);
//                b.HasOne(p => p.User)
//                    .WithOne(u => u.OfficeOwnerProfile)
//                    .HasForeignKey<OfficeOwnerProfile>(p => p.UserId);

//                b.Property(p => p.PhoneNumbers)
//                    .HasConversion(jsonConverter)
//                    .HasColumnType("NVARCHAR(MAX)");
//            });

//            builder.Entity<AdminProfile>(b =>
//            {
//                b.HasKey(p => p.UserId);
//                b.HasOne(p => p.User)
//                    .WithOne(u => u.AdminProfile)
//                    .HasForeignKey<AdminProfile>(p => p.UserId);
//            });

//            builder.Entity<SuperAdminProfile>(b =>
//            {
//                b.HasKey(p => p.UserId);
//                b.HasOne(p => p.User)
//                    .WithOne(u => u.SuperAdminProfile)
//                    .HasForeignKey<SuperAdminProfile>(p => p.UserId);
//            });

//builder.Entity<Chat>()
//    .HasMany(c => c.Messages)
//    .WithOne(m => m.Chat)
//    .HasForeignKey(m => m.ChatId)
//    .OnDelete(DeleteBehavior.Cascade);

//builder.Entity<Chat>()
//    .HasOne(c => c.Initiator)
//    .WithMany(u => u.ChatsInitiated)
//    .HasForeignKey(c => c.InitiatorId)
//    .OnDelete(DeleteBehavior.Restrict);

//builder.Entity<Chat>()
//    .HasOne(c => c.Recipient)
//    .WithMany(u => u.ChatsReceived)
//    .HasForeignKey(c => c.RecipientId)
//    .OnDelete(DeleteBehavior.Restrict);

//builder.Entity<Message>()
//    .HasOne(m => m.Sender)
//    .WithMany()
//    .HasForeignKey(m => m.SenderId)
//    .OnDelete(DeleteBehavior.Restrict);

//builder.Entity<Message>()
//    .HasMany(m => m.Replies)
//    .WithOne(t => t.ParentMessage)
//    .HasForeignKey(t => t.ParentMsgId)
//    .OnDelete(DeleteBehavior.Restrict);

//                     builder.Entity<ChatThread>()
//    .HasOne(t => t.ParentMessage)
//    .WithMany(m => m.Replies)
//    .HasForeignKey(t => t.ParentMsgId)
//    .OnDelete(DeleteBehavior.Restrict);

//            builder.Entity<Reaction>()
//                .HasOne(r => r.Message)
//                .WithMany(m => m.Reactions)
//                .HasForeignKey(r => r.MessageId)
//                .OnDelete(DeleteBehavior.Cascade);

//            builder.Entity<Reaction>()
//                .HasOne(r => r.User)
//                .WithMany()
//                .HasForeignKey(r => r.UserId)
//                .OnDelete(DeleteBehavior.Restrict);

//            builder.Entity<Attachment>()
//                .HasOne(a => a.Message)
//                .WithMany(m => m.Attachments)
//                .HasForeignKey(a => a.MessageId)
//                .OnDelete(DeleteBehavior.Cascade);


//builder.Entity<Block>()
//    .HasOne(b => b.Blocker)
//    .WithMany(u => u.BlocksInitiated)
//    .HasForeignKey(b => b.BlockerId)
//    .OnDelete(DeleteBehavior.Restrict)
//            .IsRequired();

//            builder.Entity<OfficeOwnerProfile>()
//           .HasMany(o => o.JobPosts)
//           .WithOne(j => j.OfficeOwnerProfile)
//           .HasForeignKey(j => j.OfficeOwnerProfileId)
//           .HasPrincipalKey(o => o.Id) // 💥 this line fixes the error
//           .OnDelete(DeleteBehavior.Cascade);

//            builder.Entity<JobPost>()
//            .Property(j => j.Budget)
//            .HasPrecision(18, 2); // Or adjust as needed

//            builder.Entity<Block>()
//    .HasOne(b => b.Blocked)
//    .WithMany(u => u.BlocksReceived)
//    .HasForeignKey(b => b.BlockedId)
//    .OnDelete(DeleteBehavior.Cascade)
//    .IsRequired();

//                     builder.Entity<PushSubscription>()
//                .HasOne<IdentityUser>()
//                .WithMany()
//                .HasForeignKey(ps => ps.UserId)
//                .OnDelete(DeleteBehavior.Cascade);
//        }
//    }
//}








using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OluBackendApp.Models;

namespace OluBackendApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ArtisanProfile> ArtisanProfiles { get; set; }
        public DbSet<OfficeOwnerProfile> OfficeOwnerProfiles { get; set; }
        public DbSet<AdminProfile> AdminProfiles { get; set; }
        public DbSet<SuperAdminProfile> SuperAdminProfiles { get; set; }
        public DbSet<OtpRecord> OtpRecords { get; set; }

        // Chat support
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<ChatThread> ChatThreads { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PushSubscription> PushSubscriptions { get; set; }
        public DbSet<JobPost> JobPosts { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ——— JSON-LIST CONVERTERS ———
            var jsonConverter = new ValueConverter<List<string>?, string?>(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
            );

            var listComparer = new ValueComparer<List<string>?>(
            (c1, c2) => c1 == null && c2 == null || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c == null ? 0 : c.Aggregate(0, (hash, item) => HashCode.Combine(hash, item == null ? 0 : item.GetHashCode())),
            c => c == null ? null : c.ToList()
);

            // ——— PROFILES ———
            builder.Entity<ArtisanProfile>(b =>
            {
                b.HasKey(p => p.UserId);
                b.HasOne(p => p.User)
                    .WithOne(u => u.ArtisanProfile)
                    .HasForeignKey<ArtisanProfile>(p => p.UserId);

                b.Property(p => p.PhoneNumbers)
                    .HasConversion(jsonConverter)
                    .HasColumnType("NVARCHAR(MAX)")
                    .Metadata.SetValueComparer(listComparer);

                b.Property(p => p.ServicesOffered)
                    .HasConversion(jsonConverter)
                    .HasColumnType("NVARCHAR(MAX)")
                    .Metadata.SetValueComparer(listComparer);

                b.Property(p => p.ProfessionTags)
                    .HasConversion(jsonConverter)
                    .HasColumnType("NVARCHAR(MAX)")
                    .Metadata.SetValueComparer(listComparer);
            });

            builder.Entity<OfficeOwnerProfile>(b =>
            {
                b.HasKey(p => p.UserId);
                b.HasOne(p => p.User)
                    .WithOne(u => u.OfficeOwnerProfile)
                    .HasForeignKey<OfficeOwnerProfile>(p => p.UserId);

                b.Property(p => p.PhoneNumbers)
                    .HasConversion(jsonConverter)
                    .HasColumnType("NVARCHAR(MAX)")
                    .Metadata.SetValueComparer(listComparer);

                b.HasMany(o => o.JobPosts)
                    .WithOne(j => j.OfficeOwnerProfile)
                    .HasForeignKey(j => j.OfficeOwnerProfileId)
                    .HasPrincipalKey(o => o.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AdminProfile>(b =>
            {
                b.HasKey(p => p.UserId);
                b.HasOne(p => p.User)
                    .WithOne(u => u.AdminProfile)
                    .HasForeignKey<AdminProfile>(p => p.UserId);
            });

            builder.Entity<SuperAdminProfile>(b =>
            {
                b.HasKey(p => p.UserId);
                b.HasOne(p => p.User)
                    .WithOne(u => u.SuperAdminProfile)
                    .HasForeignKey<SuperAdminProfile>(p => p.UserId);
            });

            // ——— JOB POST ———
            builder.Entity<JobPost>()
                .Property(j => j.Budget)
                .HasPrecision(18, 2);

            // ——— CHAT CONFIG ———
            builder.Entity<Chat>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Chat)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Chat>()
                .HasOne(c => c.Initiator)
                .WithMany(u => u.ChatsInitiated)
                .HasForeignKey(c => c.InitiatorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Chat>()
                .HasOne(c => c.Recipient)
                .WithMany(u => u.ChatsReceived)
                .HasForeignKey(c => c.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            // ——— MESSAGES ———
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasMany(m => m.Replies)
                .WithOne(t => t.ParentMessage)
                .HasForeignKey(t => t.ParentMsgId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatThread>()
                .HasOne(t => t.ParentMessage)
                .WithMany(m => m.Replies)
                .HasForeignKey(t => t.ParentMsgId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reaction>()
                .HasOne(r => r.Message)
                .WithMany(m => m.Reactions)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Attachment>()
                .HasOne(a => a.Message)
                .WithMany(m => m.Attachments)
                .HasForeignKey(a => a.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // ——— BLOCKS ———
            builder.Entity<Block>()
                .HasOne(b => b.Blocker)
                .WithMany(u => u.BlocksInitiated)
                .HasForeignKey(b => b.BlockerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.Entity<Block>()
                .HasOne(b => b.Blocked)
                .WithMany(u => u.BlocksReceived)
                .HasForeignKey(b => b.BlockedId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // ——— PUSH SUBSCRIPTION ———
            builder.Entity<PushSubscription>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(ps => ps.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
