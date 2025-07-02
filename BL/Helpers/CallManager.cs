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

        public static BO.Enums.CallStatus CalculateCallStatus(this DO.Call call)
        {
            lock (AdminManager.BlMutex)
            {
                var assignments = s_dal.Assignment.ReadAll(a => a?.CallId == call.ID).ToList();
                var lastAssignment = assignments.LastOrDefault(a => a?.CallId == call.ID);

                if (lastAssignment is not null && lastAssignment.EndTimeForTreatment.HasValue)
                {
                    if (lastAssignment.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.SelfCancellation || lastAssignment.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.ManagerCancellation)
                        return BO.Enums.CallStatus.opened;
                    return BO.Enums.CallStatus.closed;
                }

                if (call.MaxTimeForClosing < AdminManager.Now)
                    return BO.Enums.CallStatus.expired;

                if (lastAssignment is not null)
                {
                    if ((AdminManager.Now - lastAssignment.EntryTimeForTreatment) > s_dal.Config.RiskRange)
                        return BO.Enums.CallStatus.treated_at_risk;
                    return BO.Enums.CallStatus.is_treated;
                }

                if ((AdminManager.Now - call.OpeningTime).TotalHours > s_dal.Config.RiskRange.TotalHours)
                    return BO.Enums.CallStatus.opened_at_risk;

                return BO.Enums.CallStatus.opened;
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

                    TimeSpan? timeLeft = call.MaxTimeForClosing > DateTime.Now
                        ? call.MaxTimeForClosing - DateTime.Now
                        : null;

                    TimeSpan? totalTime = callStatus == BO.Enums.CallStatus.closed
                        ? (call.MaxTimeForClosing - call.OpeningTime)
                        : null;

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

            // Retrieve all assignments as a concrete List to avoid delayed LINQ evaluation inside a critical section
            List<DO.Assignment> assignments;
            lock (AdminManager.BlMutex)
            {
                assignments = s_dal.Assignment.ReadAll().ToList();
            }

            Console.WriteLine($"Found {assignments.Count} assignments.");
            var processedAssignments = new HashSet<int>();
            var updatedAssignmentsIds = new List<int>();

            // Iterate over all assignments and update status as needed
            foreach (var assignment in assignments)
            {
                // If the assignment ended before or at the new clock time and has not been processed yet
                if (assignment.EndTimeForTreatment <= newClock && !processedAssignments.Contains(assignment.ID))
                {
                    // Update the assignment's finish type inside a lock (DAL access)
                    var updatedAssignment = assignment with { TypeOfFinishTreatment = DO.TypeOfFinishTreatment.Treated };
                    lock (AdminManager.BlMutex)
                    {
                        s_dal.Assignment.Update(updatedAssignment);
                    }
                    // Collect the ID for notification outside the lock
                    updatedAssignmentsIds.Add(updatedAssignment.ID);
                    processedAssignments.Add(assignment.ID);
                }
                // If the assignment ends within the next 2 hours, only mark as processed (no update required)
                else if (assignment.EndTimeForTreatment <= newClock.AddHours(2) && !processedAssignments.Contains(assignment.ID))
                {
                    processedAssignments.Add(assignment.ID);
                }
            }

            // Notify observers about each updated assignment outside the lock
            foreach (var id in updatedAssignmentsIds)
                Observers.NotifyItemUpdated(id);

            // Notify that the assignment list has been updated (also outside the lock)
            Observers.NotifyListUpdated();
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