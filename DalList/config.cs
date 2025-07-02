using System;
using System.Runtime.CompilerServices;

namespace Dal
{
    internal static class Config
    {
        // Running identifier number for calls
        internal const int startCallId = 1000;
        private static int nextCallId = startCallId;

        internal static int NextCallId
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => nextCallId++;
        }

        // Running identifier number for assignments
        internal const int startAssignmentId = 2000;
        private static int nextAssignmentId = startAssignmentId;

        internal static int NextAssignmentId
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => nextAssignmentId++;
        }

        private static DateTime _clock = new DateTime(2025, 3, 13, 2, 45, 30);

        internal static DateTime Clock
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _clock;

            [MethodImpl(MethodImplOptions.Synchronized)]
            set => _clock = value;
        }

        private static TimeSpan _riskRange = TimeSpan.FromHours(2);

        internal static TimeSpan RiskRange
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _riskRange;

            [MethodImpl(MethodImplOptions.Synchronized)]
            set => _riskRange = value;
        }

        /// <summary>
        /// Method to reset initial values
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void Reset()
        {
            nextCallId = startCallId;
            nextAssignmentId = startAssignmentId;
            Clock = DateTime.Now;
            RiskRange = TimeSpan.FromHours(2);
        }
    }
}
