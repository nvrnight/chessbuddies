namespace ChessBuddies
{
    public static class MentionExtension
    {
        public static string Mention(this ulong id)
        {
            return $"<@{id}>";
        }
    }
}