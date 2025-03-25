using Helpers;
using static BO.Enums;

// This class represents an open call in the system with its details.

namespace BO
{
    public class OpenCallInList
    {
        /// Gets or sets the unique identifier for the open call.
        public int Id { get; set; }

        /// Gets or sets the type of the open call (e.g., emergency, general inquiry).
        public CallType CallType { get; set; }

        /// Gets or sets the verbal description of the open call.
        public string? Verbal_description { get; set; }

        /// Gets or sets the full address where the open call is taking place.
        public string? FullAddress { get; set; }

        /// Gets or sets the time when the open call started.
        public DateTime Start_time { get; set; }

        /// Gets or sets the maximum time for the open call to be finished.
        public DateTime? Max_finish_time { get; set; }

        /// Gets or sets the distance for the open call in relation to the volunteer.
        public double CallDistance { get; set; }

        /// <summary>
        /// Provides a string representation of the OpenCallInList object based on its properties.
        /// </summary>
        /// <returns>A string representing the OpenCallInList object.</returns>
        public override string ToString() => this.ToStringProperty();
    }
}
