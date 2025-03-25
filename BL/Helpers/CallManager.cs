
using DalApi;

namespace Helpers
{
    internal static class CallManager
    {
        private static IDal s_dal = Factory.Get;

        /// <summary>
        /// Calculates the current status of a call based on its properties and assignments.
        /// </summary>
        /// <param name="call">The call object whose status needs to be calculated.</param>
        /// <returns>The current status of the call as an enum value.</returns>
        public static BO.Enums.CallStatus CalculateCallStatus(this DO.Call call)
        {
            List<DO.Assignment?> assignments = s_dal.Assignment.ReadAll(a => a?.CallId == call.ID).ToList()!;
            var lastAssignment = assignments.LastOrDefault(a => a!.CallId == call.ID);

            // If the maximum time for closing the call has passed
            if (call.MaxTimeForClosing < ClockManager.Now)
                return BO.Enums.CallStatus.expired;

            // If the call is open and is ending during the risk period
            if ((ClockManager.Now - call.OpeningTime).TotalHours > s_dal.Config.RiskRange.TotalHours)
                return BO.Enums.CallStatus.opened_at_risk;

            // If the call is being treated
            if (lastAssignment != null)
            {
                // Treated at risk
                if ((ClockManager.Now - lastAssignment?.EntryTimeForTreatment) > s_dal.Config.RiskRange)
                    return BO.Enums.CallStatus.treated_at_risk;

                // Just treated
                else
                    return BO.Enums.CallStatus.is_treated;
            }

            // If the call is closed (last assignment has an end time)
            if (lastAssignment is not null && lastAssignment.EndTimeForTreatment.HasValue)
                return BO.Enums.CallStatus.closed;

            // If the call is open
            return BO.Enums.CallStatus.opened;
        }

        /// <summary>
        /// Converts a collection of DO.Call objects to a collection of BO.CallInList objects.
        /// </summary>
        /// <param name="calls">The list of DO.Call objects to be converted.</param>
        /// <returns>A collection of BO.CallInList objects.</returns>
        public static IEnumerable<BO.CallInList> ConvertToCallInList(IEnumerable<DO.Call> calls)
        {
            return calls.Select(call =>
            {
                List<DO.Assignment?> assignments = s_dal.Assignment.ReadAll(a => a?.CallId == call.ID).ToList()!;
                var lastAssignment = assignments.LastOrDefault(a => a!.CallId == call.ID);
                var lastVolunteerName = lastAssignment is not null ? s_dal.Volunteer.Read(lastAssignment.VolunteerId)!.Name : null;
                TimeSpan? timeLeft = call.MaxTimeForClosing > DateTime.Now ? call.MaxTimeForClosing - DateTime.Now : null;
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

        /// <summary>
        /// Validates the properties of a BO.Call object to ensure they are correctly formatted.
        /// </summary>
        /// <param name="call">The call object to be validated.</param>
        /// <exception cref="BO.BlInvalidFormatException">Thrown if any property is invalid.</exception>
        public static void ValidateCall(BO.Call call)
        {
            if (call is null)
                throw new BO.BlInvalidFormatException("Call cannot be null!");

            // Validate the address
            if (string.IsNullOrWhiteSpace(call.FullAddress))
                throw new BO.BlInvalidFormatException("Invalid address!");

            // Validate the description
            if (string.IsNullOrWhiteSpace(call.Verbal_description))
                throw new BO.BlInvalidFormatException("Invalid description!");

            // Validate the opening time
            if (call.Opening_time == default)
                throw new BO.BlInvalidFormatException("Invalid opening time!");

            // Validate the max finish time
            if (call.Max_finish_time != default && call.Max_finish_time <= call.Opening_time)
                throw new BO.BlInvalidFormatException("Invalid max finish time! Finish time has to be bigger than opening time.");
        }

        /// <summary>
        /// Converts a BO.Call object to a DO.Call object for database storage.
        /// </summary>
        /// <param name="call">The BO.Call object to be converted.</param>
        /// <returns>A DO.Call object corresponding to the BO.Call object.</returns>
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
