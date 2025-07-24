//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;

//using OluBackendApp.Models;  // for MessageStatus

//namespace OluBackendApp.DTOs
//{

//    public enum MessageStatus
//    {
//        Sent,
//        Delivered,
//        Read
//    }
//    public record CreateChatDto(
//        [Required] string RecipientId
//    );

//    public record ChatSummaryDto(
//        int Id,
//        string InitiatorId,
//        string RecipientId,
//        DateTime CreatedAt,
//        string? LastMessage,
//        DateTime? LastSentAt

//        //int UnreadCount,
//        //bool OtherIsTyping,
//        //PresenceStatus OtherStatus,
//        //DateTime? OtherLastSeen
//    );

//    public record CreateMessageDto(
//        [Required, StringLength(2000)] string Content
//    );

//    public record UpdateMessageDto(
//        [Required, StringLength(2000)] string NewContent
//    );

//    public record MessageDto(
//        int Id,
//        string SenderId,
//        string ContentHtml,
//        DateTime SentAt,
//        MessageStatus Status,       // ← uses the enum from Models
//        DateTime? DeliveredAt,
//        DateTime? ReadAt,
//        bool IsEdited,
//        DateTime? EditedAt,
//        bool IsDeleted
//    );

//    public record ReactionDto(
//        int MessageId,
//        string UserId,
//        string Emoji
//    );

//    public record ThreadDto(
//        int ThreadId,
//        int ParentMsgId,
//        IEnumerable<MessageDto> Replies
//    );

//    public record BlockUserDto(
//        [Required] string UserId
//    );

//    //public enum PresenceStatus
//    //{
//    //    Online,
//    //    Offline,
//    //    Away
//    //}


//}









using System.ComponentModel.DataAnnotations;

namespace OluBackendApp.DTOs
{
    public enum MessageStatus
    {
        Sent,
        Delivered,
        Read
    }

    public record CreateMessageDto(
        [Required, StringLength(2000)]
        string Content
    );

    public record UpdateMessageDto(
        [Required, StringLength(2000)]
        string NewContent
    );

    public record MessageDto(
        int Id,
        string SenderId,
        string ContentHtml,
        DateTime SentAt,
        MessageStatus Status,
        DateTime? DeliveredAt,
        DateTime? ReadAt,
        bool IsEdited,
        DateTime? EditedAt,
        bool IsDeleted
    );

    public record CreateChatDto(
        [Required]
        string RecipientId
    );

    public record ChatSummaryDto(
        int Id,
        string InitiatorId,
        string RecipientId,
        DateTime CreatedAt,
        string? LastMessage,
        DateTime? LastSentAt
    );

    public record ReactionDto(
        int MessageId,
        string UserId,
        string Emoji
    );

    public record ThreadDto(
        int ThreadId,
        int ParentMsgId,
        IEnumerable<MessageDto> Replies
    );

    public record BlockUserDto(
        [Required]
        string UserId
    );
}
