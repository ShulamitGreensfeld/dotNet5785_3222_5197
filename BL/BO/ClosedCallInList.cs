using Helpers;
using Microsoft.VisualBasic;
using static BO.Enums;
// This class represents a call that has been closed, with details about its type, timing, and status.

namespace BO
{
    public class ClosedCallInList
    {
        /// Gets or sets the unique identifier of the closed call.
        public int Id { get; set; }

        /// Gets or sets the type of the closed call (e.g., emergency, general inquiry).
        public Enums.CallType CallType { get; set; }

        /// Gets or sets the full address where the call occurred.
        public string? FullAddress { get; set; }

        /// Gets or sets the time when the call was opened.
        public DateTime Opening_time { get; set; }

        /// Gets or sets the time when the call actually started.
        public DateTime Start_time { get; set; }

        /// Gets or sets the time when the call was closed.
        public DateTime? End_time { get; set; }

        /// Gets or sets the type of ending for the closed call (e.g., successful, failed).
        public EndType? EndType { get; set; }

        /// <summary>
        /// Provides a string representation of the ClosedCallInList object based on its properties.
        /// </summary>
        /// <returns>A string representing the ClosedCallInList object.</returns>
        public override string ToString() => this.ToStringProperty();
    }
}
