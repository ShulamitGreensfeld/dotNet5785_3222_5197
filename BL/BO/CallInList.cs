using Helpers;
using Microsoft.VisualBasic;
using static BO.Enums;
// This class represents the call information in a list format, including the call's status, type, and associated volunteer details.
namespace BO
{
    public class CallInList
    {
        /// Gets or sets the unique identifier of the call in the list.
        public int? Id { get; set; }

        /// Gets or sets the identifier of the related call.
        public int CallId { get; set; }

        /// Gets or sets the type of the call (e.g., emergency, general inquiry).
        public Enums.CallType CallType { get; set; }

        /// Gets or sets the time when the call was opened.
        public DateTime Opening_time { get; set; }

        /// Gets or sets the remaining time until the call's expiration.
        public TimeSpan? TimeLeft { get; set; }

        /// Gets or sets the name of the last volunteer assigned to the call.
        public string? LastVolunteerName { get; set; }

        /// Gets or sets the total time spent on the call.
        public TimeSpan? TotalTime { get; set; }

        /// Gets or sets the status of the call (e.g., opened, closed, assigned).
        public CallStatus CallStatus { get; set; }

        /// Gets or sets the total number of assignments made to the call.
        public int TotalAssignments { get; set; }

        /// <summary>
        /// Provides a string representation of the CallInList object based on its properties.
        /// </summary>
        /// <returns>A string representing the CallInList object.</returns>
        public override string ToString() => this.ToStringProperty();
    }
}

