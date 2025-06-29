namespace BlApi
{
    public interface ICall :IObservable //stage 5
    {
        /// <summary>
        /// Gets the quantity of calls by their status.
        /// </summary>
        /// <returns>An array of integers representing the number of calls in each status.</returns>
        int[] GetCallQuantitiesByStatus();

        /// <summary>
        /// Gets a list of calls with optional filtering and sorting.
        /// </summary>
        /// <param name="fieldFilter">An optional field to filter the calls by.</param>
        /// <param name="filterValue">An optional value for the filter field.</param>
        /// <param name="sortField">An optional field to sort the calls by.</param>
        /// <returns>A collection of calls based on the specified filters and sorting.</returns>
        IEnumerable<BO.CallInList> GetCallsList(BO.Enums.CallInListFields? fieldFilter = null, object? filterValue = null, BO.Enums.CallInListFields? sortField = null);

        /// <summary>
        /// Gets the detailed information of a specific call.
        /// </summary>
        /// <param name="callId">The ID of the call to retrieve details for.</param>
        /// <returns>The call details.</returns>
        BO.Call GetCallDetails(int callId);

        /// <summary>
        /// Updates the details of a specific call.
        /// </summary>
        /// <param name="call">The updated call object with new details.</param>
        void UpdateCallDetails(BO.Call call);

        /// <summary>
        /// Deletes a specific call by its ID.
        /// </summary>
        /// <param name="callId">The ID of the call to delete.</param>
        void DeleteCall(int callId);

        /// <summary>
        /// Adds a new call to the system.
        /// </summary>
        /// <param name="call">The call object to be added.</param>
        void AddCall(BO.Call call);

        /// <summary>
        /// Gets a list of closed calls that were handled by a specific volunteer.
        /// </summary>
        /// <param name="volunteerId">The ID of the volunteer who handled the calls.</param>
        /// <param name="callTypeFilter">An optional filter to limit calls by type.</param>
        /// <param name="sortField">An optional field to sort the closed calls by.</param>
        /// <returns>A collection of closed calls handled by the volunteer.</returns>
        IEnumerable<BO.ClosedCallInList> GetClosedCallsHandledByVolunteer(int volunteerId, BO.Enums.CallType? callTypeFilter = null, BO.Enums.ClosedCallInListFields? sortField = null);

        /// <summary>
        /// Gets a list of open calls assigned to a specific volunteer.
        /// </summary>
        /// <param name="volunteerId">The ID of the volunteer who is assigned the open calls.</param>
        /// <param name="callTypeFilter">An optional filter to limit calls by type.</param>
        /// <param name="sortField">An optional field to sort the open calls by.</param>
        /// <returns>A collection of open calls assigned to the volunteer.</returns>
        //IEnumerable<BO.OpenCallInList> GetOpenCallsForVolunteer(int volunteerId, BO.Enums.CallType? callTypeFilter = null, BO.Enums.OpenCallInListFields? sortField = null);

        /// <summary>
        /// Marks a call as completed by a specific volunteer and assignment.
        /// </summary>
        /// <param name="volunteerId">The ID of the volunteer completing the call.</param>
        /// <param name="assignmentId">The ID of the assignment associated with the call.</param>
        void MarkCallCompletion(int volunteerId, int assignmentId);

        /// <summary>
        /// Marks a call as cancelled by a specific volunteer and assignment.
        /// </summary>
        /// <param name="volunteerId">The ID of the volunteer cancelling the call.</param>
        /// <param name="assignmentId">The ID of the assignment associated with the call.</param>
        void MarkCallCancellation(int volunteerId, int assignmentId);

        /// <summary>
        /// Selects a call for treatment by a specific volunteer.
        /// </summary>
        /// <param name="volunteerId">The ID of the volunteer selecting the call.</param>
        /// <param name="callId">The ID of the call being selected for treatment.</param>
        void SelectCallForTreatment(int volunteerId, int callId);

        Task<IEnumerable<BO.OpenCallInList>> GetOpenCallsForVolunteerAsync(int volunteerId, BO.Enums.CallType? callTypeFilter = null, BO.Enums.OpenCallInListFields? sortField = null);
    }
}
