namespace RealTimeChat.API.Hubs;

public static class HubEvents
{
    // ── Messages ─────────────────────────────────────────────────────────────
    public const string ReceiveMessage = "ReceiveMessage";
    public const string MessageEdited = "MessageEdited";
    public const string MessageDeleted = "MessageDeleted";
    public const string MessageDelivered = "MessageDelivered";
    public const string MessageSeen = "MessageSeen";

    // ── Typing ───────────────────────────────────────────────────────────────
    public const string UserTyping = "UserTyping";
    public const string UserStoppedTyping = "UserStoppedTyping";

    // ── Presence ─────────────────────────────────────────────────────────────
    public const string UserOnline = "UserOnline";
    public const string UserOffline = "UserOffline";

    // ── Chat Room ─────────────────────────────────────────────────────────────
    public const string JoinedChat = "JoinedChat";
    public const string LeftChat = "LeftChat";

    public const string GroupCreated = "GroupCreated";
    public const string MemberAdded = "MemberAdded";
    public const string MemberRemoved = "MemberRemoved";

    // ── Errors ───────────────────────────────────────────────────────────────
    public const string Error = "Error";
}