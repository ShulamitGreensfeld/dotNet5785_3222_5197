namespace BlApi
{
    public interface IVolunteer
    {
        /// <summary>
        /// Authenticates and enters the system with a specific name and password.
        /// </summary>
        /// <param name="name">The name of the volunteer entering the system.</param>
        /// <param name="pass">The password for the volunteer.</param>
        /// <returns>The role of the volunteer after authentication.</returns>
        BO.Enums.Role EnterSystem(string name, string pass);

        /// <summary>
        /// Retrieves a list of volunteers with optional filtering by active status or sorting by a field.
        /// </summary>
        /// <param name="isActive">An optional filter to return only active or inactive volunteers.</param>
        /// <param name="fieldFilter">An optional field to filter the volunteers by.</param>
        /// <returns>A collection of volunteers based on the filtering criteria.</returns>
        IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActive = null, BO.Enums.VolunteerInListFields? fieldFilter = null);

        /// <summary>
        /// Gets the details of a volunteer by ID and password.
        /// </summary>
        /// <param name="id">The ID of the volunteer.</param>
        /// <param name="password">The password of the volunteer.</param>
        /// <returns>The volunteer details if the ID and password are correct; otherwise, null.</returns>
        BO.Volunteer GetVolunteerDetails(int id);

        /// <summary>
        /// Updates the details of a specific volunteer based on the given volunteer object.
        /// </summary>
        /// <param name="id">The ID of the volunteer to update.</param>
        /// <param name="volunteer">The updated volunteer object containing new details.</param>
        void UpdateVolunteerDetails(int id, BO.Volunteer volunteer);

        /// <summary>
        /// Deletes a specific volunteer by their ID.
        /// </summary>
        /// <param name="id">The ID of the volunteer to delete.</param>
        void DeleteVolunteer(int id);

        /// <summary>
        /// Adds a new volunteer to the system.
        /// </summary>
        /// <param name="volunteer">The volunteer object to add.</param>
        void AddVolunteer(BO.Volunteer volunteer);
    }
}
