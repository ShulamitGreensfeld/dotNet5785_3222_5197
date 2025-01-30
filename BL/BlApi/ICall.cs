namespace BlApi;

public interface ICall
{
    int[] GetCallsCount();
    IEnumerable<BO.CallInList> GetCallsList(BO.CallField? filterBy, object? filterValue, BO.CallField? sortBy);
    BO.Call GetCallDetails(int id);
    void UpdateCallDetails(BO.Call boCall);
    void Delete(int callId);
    void AddCall(BO.Call boCall);
    IEnumerable<BO.ClosedCallInList> GetClosedCallsHandledByTheVolunteer(int volunteerId, BO.TypeOfCall? callTypeFilter, BO.CallField? sortBy);
    IEnumerable<BO.OpenCallInList> GetOpenCallsCanBeSelectedByAVolunteer(int volunteerId, BO.TypeOfCall? callTypeFilter, BO.CallField? sortBy);
    void TreatmentCompletionUpdate(int volunteerId, int AssignmentId);
    void TreatmentCancellationUpdate(int volunteerId, int AssignmentId);
    void ChoosingACallForTreatment(int volunteerId, int CallId);
}