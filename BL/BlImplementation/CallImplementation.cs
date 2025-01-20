
using BlApi;
using BO;
using Helpers;

namespace BlImplementation
{
    internal class CallImplementation : ICall
    {
        private readonly DalApi.IDal Call_dal = DalApi.Factory.Get;
        private void ValidateCall(BO.Call boCall)
        {
            if (boCall == null)
                throw new ArgumentNullException(nameof(boCall), "The call object cannot be null.");

            // Check if the ID is valid
            if (boCall.Id <= 0)
                throw new ArgumentException("Call ID must be a positive number.");

            // Validate time constraints
            if (boCall.MaxTimeForClosing.HasValue && boCall.MaxTimeForClosing.Value <= boCall.OpeningTime)
                throw new ArgumentException("End time must be greater than the start time.");

            // Check if the address is valid
            if (string.IsNullOrWhiteSpace(boCall.Address))
                throw new ArgumentException("Address cannot be null or empty.");

            // Validate the address against a geolocation service
            (double lat, double lon) = CallManager.GetCoordinates(boCall.Address);

            if (lat == null || lon == null)
                throw new ArgumentException("Address is invalid or could not be found.");

            // Assign valid coordinates
            boCall.Latitude = lat;
            boCall.Longitude = lon;

            // Check for other business logic conditions if needed
            if (boCall.CallStatus == BO.CallStatus.closed && boCall.OpeningTime > DateTime.Now)
                throw new ArgumentException("A closed call cannot have a start time in the future.");
        }
        public void ChoosingACallForTreatment(int volunteerId, int callId)
        {
            var call = Call_dal.Call.Read(callId)
                ?? throw new BO.EntityNotFoundException("קריאה לא נמצאה במערכת.");

            if (call.Status != DO.TypeOfFinishTreatment.Treated && call.Status != DO.TypeOfFinishTreatment.OutOfRangeCancellation)
                throw new BO.InvalidOperationException("קריאה זו אינה פתוחה לטיפול.");

            if (Call_dal.Assignment.ReadAll().Any(a => a.CallId == callId))
                throw new BO.InvalidOperationException("קריאה זו כבר בטיפול.");

            var newAssignment = new DO.Assignment
            {
                VolunteerId = volunteerId,
                CallId = callId,
                EntryTimeForTreatment = ClockManager.Now,
                EndTimeForTreatment = null,
            };

            Call_dal.Assignment.Create(newAssignment);
        }

        public void Create(BO.Call boCall)
        {
            ValidateCall(boCall);

            var doCall = new DO.Call
            {
                ID = boCall.Id,
                CallDescription = boCall.CallDescription,
                Address = boCall.Address,
                Latitude = boCall.Latitude,
                Longitude = boCall.Longitude,
                OpeningTime = boCall.OpeningTime,
                MaxTimeForClosing = boCall.MaxTimeForClosing
            };

            Call_dal.Call.Create(doCall);
        }

        public void Delete(int id)
        {
            var call = Call_dal.Call.Read(id)
                ?? throw new BO.EntityNotFoundException("קריאה לא נמצאה במערכת.");

            if (call.Status != DO.TypeOfFinishTreatment.Treated || Call_dal.Assignment.ReadAll().Any(a => a.CallId == id))
                throw new BO.InvalidOperationException("לא ניתן למחוק קריאה זו.");

            Call_dal.Call.Delete(id);
        }

        public BO.Call GetCallDetails(int id)
        {
            var doCall = Call_dal.Call.Read(id)
                ?? throw new BO.EntityNotFoundException("קריאה לא נמצאה במערכת.");

            var assignments = Call_dal.Assignment.ReadAll()
                .Where(a => a.CallId == id)
                .Select(a => new BO.CallAssignInList
                {
                    VolunteerId = a.VolunteerId,
                    EntranceTime = a.TreatmentStartTime,
                    FinishingTime = a.TreatmentEndTime,
                    //Status = (BO.Status)a.Status
                }).ToList();

            return new BO.Call
            {
                Id = doCall.Id,
                CallDescription = doCall.Description,
                CallStatus = (BO.CallStatus)doCall.Status,
                Address = doCall.Address,
                Latitude = doCall.Latitude,
                Longitude = doCall.Longitude,
                OpeningTime = doCall.OpeningTime,
                MaxTimeForClosing = doCall.MaxClosingTime,
                AssignedVolunteers = assignments
            };
        }

        public IEnumerable<int> GetCallsCount()
        {
            return Call_dal.Call.ReadAll()
                .GroupBy(c => c.CallDescription)
                .OrderBy(g => g.Key)
                .Select(g => g.Count())
                .ToArray();
        }

        public IEnumerable<BO.CallInList> GetCallsList(Enum? filterBy, object? filter, Enum? sortBy)
        {
            var calls = Call_dal.Call.ReadAll();

            if (filterBy != null && filter != null)
            {
                calls = calls.Where(c => MatchField(filterBy, c, filter));
            }

            var callList = calls.Select(c => new BO.CallInList
            {
                Id = c.ID,
                //Description = c.Description,
                CallStatus = (BO.CallStatus)c.CallStatus,
                //Address = c.Address
            });

            if (sortBy != null)
            {
                callList = SortByField(sortBy, callList);
            }

            return callList.ToList();
        }

        public IEnumerable<BO.ClosedCallInList> GetClosedCallsHandledByTheVolunteer(int volunteerId, Enum? sortBy)
        {
            var calls = Call_dal.Assignment.ReadAll()
                .Where(a => a.VolunteerId == volunteerId && a.AssignmentStatus == DO.AssignmentStatus.CLOSED)
                .Select(a => a.CallId)
                .Distinct()
                .Select(id => Call_dal.Call.Read(id))
                .Where(c => c.Status == DO.CallStatus.CLOSED);

            var closedCalls = calls.Select(c => new BO.ClosedCallInList
            {
                Id = c.Id,
                //Name = c.Name,
                Address = c.Address
            });

            if (sortBy != null)
            {
                closedCalls = SortByField(sortBy, closedCalls);
            }

            return closedCalls.ToList();
        }

        private bool MatchField(Enum? filterBy, DO.Call call, object? filterValue)
        {
            if (filterBy == null)
                return true;

            // Match by the field defined in the filterBy enum
            switch (filterBy)
            {
                case BO.CallField.STATUS:
                    return call.Status.Equals(filterValue);
                case BO.CallField.PRIORITY:
                    return call.riskRange.Equals(filterValue);
                case BO.CallField.TYPE:
                    return call.TypeOfCall.Equals(filterValue);
                default:
                    throw new ArgumentException("Unsupported filter field", nameof(filterBy));
            }
        }

        private IEnumerable<BO.OpenCallInList> SortByField(Enum? sortBy, IEnumerable<BO.OpenCallInList> openCalls)
        {
            if (sortBy == null)
                return openCalls.OrderBy(c => c.Id); // Default sort by ID

            // Sort by the field defined in the sortBy enum
            return sortBy switch
            {
                BO.CallField.ADDRESS => openCalls.OrderBy(c => c.Address),
                BO.CallField.CALL_VOLUNTEER_DISTANCE => openCalls.OrderBy(c => c.CallVolunteerDistance),
                BO.CallField.ID => openCalls.OrderBy(c => c.Id),
                _ => throw new ArgumentException("Unsupported sort field", nameof(sortBy)),
            };
        }

        public IEnumerable<BO.OpenCallInList> GetOpenCallsCanBeSelectedByAVolunteer(int volunteerId, Enum? filterBy, Enum? sortBy)
        {
            var calls = Call_dal.Call.ReadAll()
                .Where(c => c.Status == DO.CallStatus.OPEN || c.Status == DO.CallStatus.OPEN_IN_RISK);

            if (filterBy != null)
            {
                calls = calls.Where(c => MatchField(filterBy, c, null));
            }

            var openCalls = calls.Select(c => new BO.OpenCallInList
            {
                Id = c.Id,
                Address = c.Address,
                CallVolunteerDistance = CallManager.GetAerialDistance(Call_dal.Volunteer.Read(volunteerId).Address, c.Address)
            });

            if (sortBy != null)
            {
                openCalls = SortByField(sortBy, openCalls);
            }

            return openCalls.ToList();
        }

        public void TreatmentCancellationUpdate(int volunteerId, int assignmentId)
        {
            var assignment = Call_dal.Assignment.Read(assignmentId)
                ?? throw new BO.EntityNotFoundException("הקצאה לא נמצאה במערכת.");

            //if (assignment.Status != DO.AssignmentStatus.Open)
            //    throw new BO.InvalidOperationException("לא ניתן לבטל הקצאה שכבר טופלה או שפג תוקפה.");

            //assignment.Status = assignment.VolunteerId == volunteerId ?
            //    DO.AssignmentStatus.CanceledByVolunteer :
            //    DO.AssignmentStatus.CanceledByManager;

            assignment.TreatmentEndTime = ClockManager.Now;

            Call_dal.Assignment.Update(assignment);
        }

        public void TreatmentCompletionUpdate(int volunteerId, int assignmentId)
        {
            var assignment = Call_dal.Assignment.Read(assignmentId)
                ?? throw new BO.EntityNotFoundException("הקצאה לא נמצאה במערכת.");

            if (assignment.AssignmentStatus != DO.AssignmentStatus.OPEN)
                throw new BO.InvalidOperationException("לא ניתן לעדכן סיום לטיפול שכבר טופל או שפג תוקפו.");

            if (assignment.VolunteerId != volunteerId)
                throw new BO.UnauthorizedOperationException("רק המתנדב שהוקצה יכול לעדכן סיום טיפול.");

            assignment.Status = DO.AssignmentStatus.COMPLETED;
            assignment.TreatmentEndTime = ClockManager.Now;

            Call_dal.Assignment.Update(assignment);
        }

        public void Update(BO.Call boCall)
        {
            ValidateCall(boCall);

            var doCall = Call_dal.Call.Read(boCall.Id)
                ?? throw new BO.EntityNotFoundException("קריאה לא נמצאה במערכת.");

            doCall.Description = boCall.Description;
            doCall.Status = (DO.CallStatus)boCall.Status;
            doCall.Address = boCall.Address;
            doCall.Latitude = boCall.Latitude;
            doCall.Longitude = boCall.Longitude;
            doCall.OpeningTime = boCall.OpeningTime;
            doCall.MaxClosingTime = boCall.MaxClosingTime;

            Call_dal.Call.Update(doCall);
        }

        public int[] GetCallQuantitiesByStatus()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CallInList> GetCallList(TypeOfCall? filterField = null, object? filterValue = null, CallStatus? sortField = null)
        {
            throw new NotImplementedException();
        }

        public void UpdateCallDetails(Call call)
        {
            throw new NotImplementedException();
        }

        public void DeleteCall(int callId)
        {
            throw new NotImplementedException();
        }

        public void AddCall(Call call)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ClosedCallInList> GetClosedCallsByVolunteer(int volunteerId, TypeOfCall? callStatus = null, TypeOfFinishTreatment? sortField = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CallInProgress> GetOpenCallsForVolunteer(int volunteerId, TypeOfCall? callType = null, CallStatus? sortField = null)
        {
            throw new NotImplementedException();
        }

        public void UpdateCallCompletion(int volunteerId, int assignmentId)
        {
            throw new NotImplementedException();
        }

        public void UpdateCallCancellation(int volunteerId, int assignmentId)
        {
            throw new NotImplementedException();
        }

        public void SelectCallForTreatment(int volunteerId, int callId)
        {
            throw new NotImplementedException();
        }

        // Helper methods for validation, filtering, sorting, and distance calculation...
    }
}