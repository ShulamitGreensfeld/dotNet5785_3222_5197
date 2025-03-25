using Helpers;
using Microsoft.VisualBasic;
using static BO.Enums;
// This class represents a Call entity with various properties such as ID, call type, description, 
// address details, status, and associated assignments. It also includes a ToString method to return 
// a string representation of the object based on its properties.
namespace BO
{
    public class Call
    {
        /// Gets or sets the unique identifier for the call.
        public int Id { get; set; }

        /// Gets or sets the type of the call (e.g., emergency, non-emergency).
        public Enums.CallType CallType { get; set; }

        /// Gets or sets the verbal description of the call.
        public string? Verbal_description { get; set; }

        /// Gets or sets the full address related to the call.
        public string? FullAddress { get; set; }

        /// Gets or sets the latitude of the location for the call.
        public double? Latitude { get; set; }

        /// Gets or sets the longitude of the location for the call.
        public double? Longitude { get; set; }

        /// Gets or sets the opening time of the call.
        public DateTime Opening_time { get; set; }

        /// Gets or sets the maximum finish time for the call.
        public DateTime Max_finish_time { get; set; }

        /// Gets or sets the current status of the call (e.g., open, closed).
        public CallStatus CallStatus { get; set; }

        /// Gets or sets the list of assignments related to the call.
        public List<BO.CallAssignInList>? AssignmentsList { get; set; }

        /// <summary>
        /// Provides a string representation of the Call object based on its properties.
        /// </summary>
        /// <returns>A string representing the Call object.</returns>
        public override string ToString() => this.ToStringProperty();
    }
}
