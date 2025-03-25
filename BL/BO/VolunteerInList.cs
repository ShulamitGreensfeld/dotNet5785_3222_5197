using Helpers;
using static BO.Enums;
namespace BO
{
    public class VolunteerInList
    {
        /// Gets or sets the unique identifier of the volunteer.
        public int Id { get; set; }

        /// Gets or sets the full name of the volunteer.
        public string? FullName { get; set; }

        /// Gets or sets whether the volunteer is currently active.
        public bool IsActive { get; set; }

        /// Gets or sets the total number of calls handled by the volunteer.
        public int TotalHandledCalls { get; set; }

        /// Gets or sets the total number of calls canceled by the volunteer.
        public int TotalCanceledCalls { get; set; }

        /// Gets or sets the total number of expired calls (calls not completed on time) for the volunteer.
        public int TotalExpiredCalls { get; set; }

        /// Gets or sets the ID of the current call assigned to the volunteer, if any.
        public int? CallId { get; set; }

        /// Gets or sets the type of the current call assigned to the volunteer.
        public CallType CallType { get; set; }

        /// <summary>
        /// Provides a string representation of the VolunteerInList object based on its properties.
        /// </summary>
        /// <returns>A string representing the VolunteerInList object.</returns>
        public override string ToString() => this.ToStringProperty();
    }
}
