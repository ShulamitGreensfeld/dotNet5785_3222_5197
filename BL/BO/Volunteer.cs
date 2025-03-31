using Helpers;
using static BO.Enums;
namespace BO
{
    public class Volunteer
    {
        /// Gets the unique identifier for the volunteer.
        public int Id { get; init; }

        /// Gets or sets the full name of the volunteer.
        public string FullName { get; set; }

        /// Gets or sets the cellphone number of the volunteer.
        public string CellphoneNumber { get; set; }

        /// Gets or sets the email address of the volunteer.
        public string Email { get; set; }

        /// Gets or sets the password of the volunteer. It's nullable as it may not be set initially.
        public string? Password { get; set; }

        /// Gets or sets the full address of the volunteer.
        public string? FullAddress { get; set; }

        /// Gets or sets the latitude of the volunteer's location.
        public double? Latitude { get; set; }

        /// Gets or sets the longitude of the volunteer's location.
        public double? Longitude { get; set; }

        /// Gets or sets the role of the volunteer (e.g., Manager, Volunteer).
        public Role Role { get; set; }

        /// Gets or sets whether the volunteer is active or not.
        public bool IsActive { get; set; }

        /// Gets or sets the distance type for the volunteer (e.g., kilometers, miles).
        public DistanceTypes DistanceType { get; set; }

        /// Gets or sets the maximum distance the volunteer can cover, depending on the distance type.
        public double? MaxDistance { get; set; }

        /// Gets or sets the total number of calls handled by the volunteer.
        public int TotalHandledCalls { get; set; }

        /// Gets or sets the total number of calls canceled by the volunteer.
        public int TotalCanceledCalls { get; set; }

        /// Gets or sets the total number of expired calls (calls not completed on time).
        public int TotalExpiredCalls { get; set; }

        /// Gets or sets the current call in progress for the volunteer, if any.
        public BO.CallInProgress? CallInProgress { get; set; }

        /// <summary>
        /// Provides a string representation of the Volunteer object based on its properties.
        /// </summary>
        /// <returns>A string representing the Volunteer object.</returns>
        public override string ToString()
        {
            return $"Id: {Id}, FullName: {FullName}, CellphoneNumber: {CellphoneNumber}, Email: {Email}," +
                $" FullAddress: {FullAddress}, Latitude: {Latitude}, Longitude: {Longitude}, Role: {Role}," +
                $" IsActive: {IsActive}, DistanceType: {DistanceType}, MaxDistance: {MaxDistance}," +
                $" TotalHandledCalls: {TotalHandledCalls}, TotalCanceledCalls: {TotalCanceledCalls}, TotalExpiredCalls: {TotalExpiredCalls}, CallInProgress: {CallInProgress}";
        }
    }
}
