using Helpers;
using static BO.Enums;
// This class represents an assignment of a volunteer to a call with information about the volunteer's 
// details, the time the assignment started, the time it ended, and the type of assignment completion.
namespace BO
{
    public class CallAssignInList
    {
        /// Gets or sets the unique identifier of the volunteer assigned to the call.
        public int? VolunteerId { get; set; }

        /// Gets or sets the full name of the volunteer assigned to the call.
        public string? VolunteerFullName { get; set; }

        /// Gets or sets the time when the volunteer started handling the call.
        public DateTime Start_time { get; set; }

        /// Gets or sets the time when the volunteer finished handling the call. This can be null if the call is still ongoing.
        public DateTime? End_time { get; set; }

        /// Gets or sets the type of completion for the call, indicating how the volunteer ended the assignment (e.g., successful, canceled).
        public EndType? EndType { get; set; }

        /// <summary>
        /// Provides a string representation of the CallAssignInList object based on its properties.
        /// </summary>
        /// <returns>A string representing the CallAssignInList object.</returns>
        public override string ToString() => this.ToStringProperty();
    }
}
