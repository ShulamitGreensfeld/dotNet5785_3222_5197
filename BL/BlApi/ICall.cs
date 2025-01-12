
namespace BlApi;

public interface ICall
{
    // מתודת בקשת כמויות קריאות
    public int[] GetCallQuantitiesByStatus();

    // מתודת בקשת רשימת קריאות
    public IEnumerable<BO.CallInList> GetCallList(
        BO.TypeOfCall? filterField = null,
        object? filterValue = null,
        BO.CallStatus? sortField = null
    );

    // מתודת בקשת פרטי קריאה
    public BO.Call GetCallDetails(int callId);

    // מתודת עדכון פרטי קריאה
    public void UpdateCallDetails(BO.Call call);

    // מתודת מחיקת קריאה
    public void DeleteCall(int callId);

    // מתודת הוספת קריאה
    public void AddCall(BO.Call call);

    // מתודת בקשת רשימת קריאות סגורות שטופלו על ידי מתנדב
    public IEnumerable<BO.ClosedCallInList> GetClosedCallsByVolunteer(
        int volunteerId,
        BO.TypeOfCall? callStatus = null,
        BO.TypeOfFinishTreatment? sortField = null
    );

    // מתודת בקשת רשימת קריאות פתוחות לבחירה על ידי מתנדב
    public IEnumerable<BO.CallInProgress> GetOpenCallsForVolunteer(
        int volunteerId,
        BO.TypeOfCall? callType = null,
        BO.CallStatus? sortField = null
    );

    // מתודת עדכון "סיום טיפול" בקריאה
    public void UpdateCallCompletion(
        int volunteerId,
        int assignmentId
    );

    // מתודת עדכון "ביטול טיפול" בקריאה
    public void UpdateCallCancellation(
        int volunteerId,
        int assignmentId
    );

    // מתודת בחירת קריאה לטיפול
    public void SelectCallForTreatment(
        int volunteerId,
        int callId
    );

}
