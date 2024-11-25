namespace Dal
{
    internal static class Config
    {
        // מספר מזהה רץ לקריאה
        internal const int startCallId = 1000;
        private static int nextCallId = startCallId;
        internal static int NextCallId { get => nextCallId++; }

        // מספר מזהה רץ להקצאה
        internal const int startAssignmentId = 2000;
        private static int nextAssignmentId = startAssignmentId;
        internal static int NextAssignmentId { get => nextAssignmentId++; }

        // שעון המערכת
        internal static DateTime Clock { get; set; } = new DateTime(2025, 3, 13, 2, 45, 30);
        // טווח זמן סיכון
        internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromHours(2);

        /// <summary>
        /// מתודה לאיפוס ערכים התחלתיים
        /// </summary>
        internal static void Reset()
        {
            nextCallId = startCallId;
            nextAssignmentId = startAssignmentId;
            Clock = DateTime.Now;
            RiskRange = TimeSpan.FromHours(2); // הגדרת ערך ברירת מחדל ל-RiskRange
        }
    }
}
