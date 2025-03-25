using Helpers;
using Microsoft.VisualBasic;
using static BO.Enums;
// This class represents the call that is currently in progress with details such as call type, status, and timing.

namespace BO
{
    public class CallInProgress
    {
        /// Gets or sets the unique identifier of the call in progress.
        public int Id { get; set; }

        /// Gets or sets the identifier of the related call.
        public int CallId { get; set; }

        /// Gets or sets the type of the call (e.g., emergency, general inquiry).
        public Enums.CallType CallType { get; set; }

        /// Gets or sets the verbal description of the call.
        public string? Verbal_description { get; set; }

        /// Gets or sets the full address where the call is located.
        public string? FullAddress { get; set; }

        /// Gets or sets the time when the call was opened.
        public DateTime Opening_time { get; set; }

        /// Gets or sets the maximum time for the call to be resolved.
        public DateTime Max_finish_time { get; set; }

        /// Gets or sets the time when the call actually started.
        public DateTime Start_time { get; set; }

        /// Gets or sets the distance from the volunteer to the call's location.
        public double CallDistance { get; set; }

        /// Gets or sets the status of the call (e.g., in progress, completed, etc.).
        public CallStatus CallStatus { get; set; }

        /// <summary>
        /// Provides a string representation of the CallInProgress object based on its properties.
        /// </summary>
        /// <returns>A string representing the CallInProgress object.</returns>
        public override string ToString() => this.ToStringProperty();
    }
}
