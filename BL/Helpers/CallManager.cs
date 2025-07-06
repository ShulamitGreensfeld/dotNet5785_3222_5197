using DalApi;

namespace Helpers
{
    internal static class CallManager
    {
        private static IDal s_dal = Factory.Get; //stage 4
        internal static ObserverManager Observers = new(); //stage 5 
        private static int nextCallId = 1050;

        public static int GetNextCallId()
        {
            lock (AdminManager.BlMutex)
            {
                List<DO.Call> calls = s_dal.Call.ReadAll().ToList();
                if (calls.Any())
                {
                    nextCallId = calls.Max(c => c.ID) + 1;
                }
                return nextCallId++;
            }
        }

        //public static BO.Enums.CallStatus CalculateCallStatus(this DO.Call call)
        //{
        //    lock (AdminManager.BlMutex)
        //    {
        //        var assignments = s_dal.Assignment.ReadAll(a => a?.CallId == call.ID).ToList();
        //        var lastAssignment = assignments.LastOrDefault(a => a?.CallId == call.ID);

        //        if (lastAssignment is not null && lastAssignment.EndTimeForTreatment.HasValue)
        //        {
        //            if (lastAssignment.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.SelfCancellation ||
        //                lastAssignment.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.ManagerCancellation)
        //                return BO.Enums.CallStatus.opened;

        //            if (lastAssignment.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.OutOfRangeCancellation)
        //                return BO.Enums.CallStatus.expired;

        //            return BO.Enums.CallStatus.closed;
        //        }

        //        if (call.MaxTimeForClosing < AdminManager.Now)
        //            return BO.Enums.CallStatus.expired;

        //        if (lastAssignment is not null)
        //        {
        //            if ((AdminManager.Now - lastAssignment.EntryTimeForTreatment) > s_dal.Config.RiskRange)
        //                return BO.Enums.CallStatus.treated_at_risk;
        //            return BO.Enums.CallStatus.is_treated;
        //        }

        //        if ((AdminManager.Now - call.OpeningTime).TotalHours > s_dal.Config.RiskRange.TotalHours)
        //            return BO.Enums.CallStatus.opened_at_risk;

        //        return BO.Enums.CallStatus.opened;
        //    }
        //}
        public static BO.Enums.CallStatus CalculateCallStatus(this DO.Call call)
        {
            lock (AdminManager.BlMutex)
            {
                var assignments = s_dal.Assignment.ReadAll(a => a?.CallId == call.ID).ToList();
                var lastAssignment = assignments.LastOrDefault(a => a?.CallId == call.ID);

                if (lastAssignment is not null && lastAssignment.EndTimeForTreatment.HasValue)
                {
                    if (lastAssignment.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.SelfCancellation ||
                        lastAssignment.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.ManagerCancellation)
                        return BO.Enums.CallStatus.opened;

                    if (lastAssignment.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.OutOfRangeCancellation)
                        return BO.Enums.CallStatus.expired;

                    return BO.Enums.CallStatus.closed;
                }

                // אם הקריאה פגה תוקף
                if (call.MaxTimeForClosing < AdminManager.Now)
                    return BO.Enums.CallStatus.expired;

                // אם יש משימה פעילה
                if (lastAssignment is not null)
                {
                    var timeLeft = call.MaxTimeForClosing - AdminManager.Now;
                    if (timeLeft <= s_dal.Config.RiskRange)
                        return BO.Enums.CallStatus.treated_at_risk;
                    return BO.Enums.CallStatus.is_treated;
                }

                // קריאה פתוחה
                {
                    var timeLeft = call.MaxTimeForClosing - AdminManager.Now;
                    if (timeLeft <= s_dal.Config.RiskRange)
                        return BO.Enums.CallStatus.opened_at_risk;
                    return BO.Enums.CallStatus.opened;
                }
            }
        }

        public static IEnumerable<BO.CallInList> ConvertToCallInList(IEnumerable<DO.Call> calls)
        {
            lock (AdminManager.BlMutex)
            {
                return calls.Select(call =>
                {
                    var assignments = s_dal.Assignment.ReadAll(a => a?.CallId == call.ID).ToList();
                    var lastAssignment = assignments.LastOrDefault(a => a?.CallId == call.ID);
                    var lastVolunteerName = lastAssignment is not null
                        ? s_dal.Volunteer.Read(lastAssignment.VolunteerId)?.Name
                        : null;

                    var callStatus = CalculateCallStatus(call);

                    TimeSpan? timeLeft = null;
                    TimeSpan? totalTime = null;
                    if (callStatus == BO.Enums.CallStatus.opened || callStatus == BO.Enums.CallStatus.opened_at_risk ||
                    callStatus == BO.Enums.CallStatus.treated_at_risk || callStatus == BO.Enums.CallStatus.is_treated)
                    {
                        timeLeft = call.MaxTimeForClosing > AdminManager.Now
                            ? call.MaxTimeForClosing - AdminManager.Now
                            : TimeSpan.Zero;
                        totalTime = call.MaxTimeForClosing - call.OpeningTime;
                    }

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
                        TotalAssignments = assignments.Count
                    };
                }).ToList();
            }
        }
        public static void PeriodicVolunteersUpdates(DateTime oldClock, DateTime newClock)
        {
            Console.WriteLine("Starting PeriodicVolunteersUpdates...");

            List<DO.Assignment> assignments;
            lock (AdminManager.BlMutex)
            {
                assignments = s_dal.Assignment.ReadAll().ToList();
            }

            // עדכון משימות של קריאות שפג להן התוקף
            var expiredAssignments = assignments
                .Where(a => a.EndTimeForTreatment == null && a.TypeOfFinishTreatment == null)
                .ToList();

            foreach (var assignment in expiredAssignments)
            {
                DO.Call? call = null;
                lock (AdminManager.BlMutex)
                {
                    call = s_dal.Call.Read(assignment.CallId);
                }
                if (call != null && call.MaxTimeForClosing <= newClock)
                {
                    var updatedAssignment = assignment with
                    {
                        EndTimeForTreatment = call.MaxTimeForClosing,
                        TypeOfFinishTreatment = DO.TypeOfFinishTreatment.OutOfRangeCancellation
                    };
                    lock (AdminManager.BlMutex)
                    {
                        s_dal.Assignment.Update(updatedAssignment);
                    }
                    // עדכון המשימה
                    Observers.NotifyItemUpdated(updatedAssignment.ID);
                    // עדכון הקריאה
                    Helpers.CallManager.Observers.NotifyItemUpdated(updatedAssignment.CallId);
                    // עדכון המתנדב
                    Helpers.VolunteerManager.Observers.NotifyItemUpdated(updatedAssignment.VolunteerId);
                }
            }
            Observers.NotifyListUpdated();
            Helpers.VolunteerManager.Observers.NotifyListUpdated();
            Helpers.CallManager.Observers.NotifyListUpdated();

            var processedAssignments = new HashSet<int>();
            var updatedAssignmentsIds = new List<int>();

            foreach (var assignment in assignments)
            {
                if (assignment.EndTimeForTreatment <= newClock && !processedAssignments.Contains(assignment.ID))
                {
                    if (assignment.TypeOfFinishTreatment != DO.TypeOfFinishTreatment.OutOfRangeCancellation)
                    {
                        var updatedAssignment = assignment with { TypeOfFinishTreatment = DO.TypeOfFinishTreatment.Treated };
                        lock (AdminManager.BlMutex)
                        {
                            s_dal.Assignment.Update(updatedAssignment);
                        }
                        updatedAssignmentsIds.Add(updatedAssignment.ID);
                    }
                    processedAssignments.Add(assignment.ID);
                }
                else if (assignment.EndTimeForTreatment <= newClock.AddHours(2) && !processedAssignments.Contains(assignment.ID))
                {
                    processedAssignments.Add(assignment.ID);
                }
            }

            foreach (var id in updatedAssignmentsIds)
                Observers.NotifyItemUpdated(id);

            Observers.NotifyListUpdated();
            Helpers.VolunteerManager.Observers.NotifyListUpdated();
            Console.WriteLine("Finished Periodic the Updates successfully.");
        }
        internal static async Task SendEmailWhenCallOpenedAsync(BO.Call call)
        {
            IEnumerable<DO.Volunteer> volunteers;
            lock (AdminManager.BlMutex)
            {
                volunteers = s_dal.Volunteer.ReadAll();
            }

            foreach (var item in volunteers)
            {
                BO.Enums.DistanceTypes volunteerDistanceType = (BO.Enums.DistanceTypes)item.DistanceType;

                double distance = await Tools.CalculateDistanceAsync(
                    volunteerDistanceType,
                    item.Latitude ?? 0.0,
                    item.Longitude ?? 0.0,
                    call.Latitude ?? 0.0,
                    call.Longitude ?? 0.0
                );

                if (item.MaxDistanceForCall >= distance)
                {
                    string subject = "Opening Call";
                    string body = $@"
            Hello {item.Name},

            A new call has been opened in your area.
            Call Details:
            - Call Type: {call.CallType}
            - Call Address: {call.FullAddress}
            - Opening Time: {call.Opening_time}
            - Description: {call.Verbal_description}
            - Entry Time for Treatment: {call.Max_finish_time}
            - Call Status: {call.CallStatus}

            If you wish to handle this call, please log into the system.

            Best regards,  
            Call Management System";

                    await Tools.SendEmailAsync(item.Email, subject, body);
                }
            }
        }
        internal static async Task SendEmailToVolunteerAsync(DO.Volunteer volunteer, DO.Assignment assignment)
        {
            DO.Call call;
            lock (AdminManager.BlMutex)
            {
                call = s_dal.Call.Read(assignment.CallId)!;
            }

            string subject = "Assignment Canceled";
            string body = $@"
      Hello {volunteer.Name},

      Your assignment for handling call {assignment.ID} has been canceled by the administrator.

      Call Details:
      - Call Type: {call.TypeOfCall}
      - Call Address: {call.Address}
      - Opening Time: {call.OpeningTime}
      - Description: {call.CallDescription}
      - Entry Time for Treatment: {assignment.EntryTimeForTreatment}

      Best regards,  
      Call Management System";

            await Tools.SendEmailAsync(volunteer.Email, subject, body);
        }
        public static void ValidateCall(BO.Call call)
        {
            if (call is null)
                throw new BO.BlInvalidFormatException("Call cannot be null!");

            if (string.IsNullOrWhiteSpace(call.FullAddress))
                throw new BO.BlInvalidFormatException("Invalid address!");

            if (string.IsNullOrWhiteSpace(call.Verbal_description))
                throw new BO.BlInvalidFormatException("Invalid description!");

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
}