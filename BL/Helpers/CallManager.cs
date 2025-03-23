
using DalApi;

namespace Helpers;

internal static class CallManager
{
    private static IDal s_dal = Factory.Get;

    public static BO.Enums.CallStatus CalculateCallStatus(this DO.Call call)
    {
        List<DO.Assignment?> assignments = s_dal.Assignment.ReadAll(a => a?.CallId == call.ID).ToList()!;
        var lastAssignment = assignments.LastOrDefault(a => a!.CallId == call.ID);
        // אם עבר זמן מקסימלי לסיום הקריאה
        if (call.MaxTimeForClosing < ClockManager.Now)
            return BO.Enums.CallStatus.expired;
        // אם הקריאה פתוחה ומסיימת בזמן הסיכון
        if ((ClockManager.Now - call.OpeningTime).TotalHours > s_dal.Config.RiskRange.TotalHours)
            return BO.Enums.CallStatus.opened_at_risk;
        // אם הקריאה בטיפול
        if (lastAssignment != null)
            //בטיפול בסיכון
            if ((ClockManager.Now - lastAssignment?.EntryTimeForTreatment) > s_dal.Config.RiskRange)
                return BO.Enums.CallStatus.treated_at_risk;
            //רק בטיפול      
            else
                return BO.Enums.CallStatus.is_treated;
        if (lastAssignment is not null && lastAssignment.EndTimeForTreatment.HasValue)
            return BO.Enums.CallStatus.closed;
        return BO.Enums.CallStatus.opened;
    }
    public static IEnumerable<BO.CallInList> ConvertToCallInList(IEnumerable<DO.Call> calls)
    {
        return calls.Select(call =>
        {
            List<DO.Assignment?> assignments = s_dal.Assignment.ReadAll(a => a?.CallId == call.ID).ToList()!;
            var lastAssignment = assignments.LastOrDefault(a => a!.CallId == call.ID);
            var lastVolunteerName = lastAssignment is not null ? s_dal.Volunteer.Read(lastAssignment.VolunteerId)!.Name : null;
            TimeSpan? timeLeft = call.MaxTimeForClosing> DateTime.Now ? call.MaxTimeForClosing - DateTime.Now : null;
            BO.Enums.CallStatus callStatus = CalculateCallStatus(call);
            TimeSpan? totalTime = callStatus == BO.Enums.CallStatus.closed ? (call.MaxTimeForClosing - call.OpeningTime) : null;
            return new BO.CallInList
            {
                Id = lastAssignment?.ID,
                CallId = call.ID,
                CallType = (BO.Enums.CallType)call.TypeOfCall,
                Opening_time = call.OpeningTime,
                TimeLeft = timeLeft,
                LastVolunteerName = lastVolunteerName,
                TotalTime = totalTime,
                CallStatus = callStatus,
                TotalAssignments = assignments.Count()
            };
        }).ToList();

    }

    public static void ValidateCall(BO.Call call)
    {
        if (call is null)
            throw new BO.BlInvalidFormatException("Call cannot be null!");

        // בדיקת תקינות הכתובת
        if (string.IsNullOrWhiteSpace(call.FullAddress))
            throw new BO.BlInvalidFormatException("Invalid address!");

        if (string.IsNullOrWhiteSpace(call.Verbal_description))
            throw new BO.BlInvalidFormatException("Invalid description!");

        // בדיקת זמני קריאה
        if (call.Opening_time == default)
            throw new BO.BlInvalidFormatException("Invalid opening time!");

        if (call.Max_finish_time != default && call.Max_finish_time <= call.Opening_time)
            throw new BO.BlInvalidFormatException("Invalid max finish time! Finish time has to be bigger than opening time.");
    }

    public static DO.Call ConvertBoCallToDoCall(BO.Call call)
    {
        return new DO.Call
        {
            ID = call.Id,
            TypeOfCall = (DO.TypeOfCall)call.CallType,
            CallDescription = call.Verbal_description,
            Address = call.FullAddress!,
            Latitude = call.Latitude ?? 0.0,
            Longitude = call.Longitude ?? 0.0,
            OpeningTime = call.Opening_time,
            MaxTimeForClosing = call.Max_finish_time,
        };
    }
}