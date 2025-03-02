namespace BlApi
{
    public interface ICall
    {
        int[] GetCallQuantitiesByStatus();
        IEnumerable<BO.CallInList> GetCallsList(BO.Enums.CallInListFields? fieldFilter = null, object? filterValue = null, BO.Enums.CallInListFields? sortField = null);
        BO.Call GetCallDetails(int callId);
        void UpdateCallDetails(BO.Call call);
        void DeleteCall(int callId);
        void AddCall(BO.Call call);
        IEnumerable<BO.ClosedCallInList> GetClosedCallsHandledByVolunteer(int volunteerId, BO.Enums.CallType? callTypeFilter = null, BO.Enums.ClosedCallInListFields? sortField = null);
        IEnumerable<BO.OpenCallInList> GetOpenCallsForVolunteer(int volunteerId, BO.Enums.CallType? callTypeFilter = null, BO.Enums.OpenCallInListFields? sortField = null);
        void MarkCallCompletion(int volunteerId, int assignmentId);
        void MarkCallCancellation(int volunteerId, int assignmentId);
        void SelectCallForTreatment(int volunteerId, int callId);
    }
}