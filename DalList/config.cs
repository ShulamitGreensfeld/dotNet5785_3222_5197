namespace Dal
{
    internal static class Config
    {
        // Running identifier number for calls
        internal const int startCallId = 1000;
        private static int nextCallId = startCallId;
        internal static int NextCallId { get => nextCallId++; }

        // Running identifier number for assignments
        internal const int startAssignmentId = 2000;
        private static int nextAssignmentId = startAssignmentId;
        internal static int NextAssignmentId { get => nextAssignmentId++; }

        // System clock
        internal static DateTime Clock { get; set; } = new DateTime(2025, 3, 13, 2, 45, 30);
        // Risk range time span
        internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromHours(2);
 
        /// <summary>
        /// Method to reset initial values
        /// </summary>
        internal static void Reset()
        {
            nextCallId = startCallId;
            nextAssignmentId = startAssignmentId;
            Clock = DateTime.Now;
            RiskRange = TimeSpan.FromHours(2); // Default value for RiskRange
        }
    }
}
