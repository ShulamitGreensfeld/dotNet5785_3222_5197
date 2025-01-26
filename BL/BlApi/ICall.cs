namespace BlApi;

public interface ICall
{
    IEnumerable<int> GetCallsCount();
    IEnumerable<BO.CallInList> GetCallsList(Enum? filterBy, object? filter, Enum? sortBy);
    BO.Call GetCallDetails(int id);
    void Update(BO.Call boCall);
    void Delete(int id);
    void Create(BO.Call boCall);
    IEnumerable<BO.ClosedCallInList> GetClosedCallsHandledByTheVolunteer(int volunteerId, Enum? sortBy);
    IEnumerable<BO.OpenCallInList> GetOpenCallsCanBeSelectedByAVolunteer(int volunteerId, Enum? filterBy, Enum? sortBy);
    void TreatmentCompletionUpdate(int volunteerId, int AssignmentId);
    void TreatmentCancellationUpdate(int volunteerId, int AssignmentId);
    void ChoosingACallForTreatment(int volunteerId, int AssignmentId);
}