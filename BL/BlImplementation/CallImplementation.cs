using BO;
using DalApi;
using DO;
using Helpers;

namespace BlImplementation;

internal class CallImplementation : BlApi.ICall
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;
    private readonly bool RequesterVolunteer;

    /// <summary>
    /// Retrieves the details of a call by its ID.
    /// </summary>
    /// <param name="callId">The ID of the call.</param>
    /// <returns>A BO.Call object containing the details of the call.</returns>
    public BO.Call GetCallDetails(int callId)
    {
        try
        {
            DO.Call call;
            List<BO.CallAssignInList> callAssignInLists;
            lock (AdminManager.BlMutex)
            {
                call = _dal.Call.Read(callId) ?? throw new BO.BlDoesNotExistException($"Call with ID={callId} does Not exist");
                callAssignInLists = _dal.Assignment.ReadAll(a => a.CallId == callId)
                    .Select(a => new BO.CallAssignInList
                    {
                        VolunteerId = a.VolunteerId,
                        VolunteerFullName = _dal.Volunteer.Read(a.VolunteerId)!.Name,
                        Start_time = a.EntryTimeForTreatment,
                        End_time = a.EndTimeForTreatment,
                        EndType = (BO.Enums.EndType?)a.TypeOfFinishTreatment
                    }).ToList();
            }
            return new BO.Call
            {
                Id = call.ID,
                CallType = (BO.Enums.CallType)call.TypeOfCall,
                Verbal_description = call.CallDescription,
                FullAddress = call.Address,
                Latitude = call.Latitude,
                Longitude = call.Longitude,
                Opening_time = call.OpeningTime,
                Max_finish_time = (DateTime)call.MaxTimeForClosing!,
                CallStatus = CallManager.CalculateCallStatus(call),
                AssignmentsList = callAssignInLists,
            };
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Can not access calls", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Adds a new call to the system.
    /// </summary>
    /// <param name="boCall">The BO.Call object representing the new call.</param>
    //public void AddCall(BO.Call boCall)
    //{
    //    AdminManager.ThrowOnSimulatorIsRunning(); // stage 7
    //    DO.Call doCall;
    //    lock (AdminManager.BlMutex)
    //    {
    //        var existingCall = _dal.Call.Read(boCall.Id);
    //        if (existingCall != null)
    //            throw new BO.BlAlreadyExistException($"Call with ID={boCall.Id} already exist");
    //        CallManager.ValidateCall(boCall);
    //        boCall.Latitude = null;
    //        boCall.Longitude = null;
    //        doCall = CallManager.ConvertBoCallToDoCall(boCall);
    //        _dal.Call.Create(doCall);
    //    }
    //    _=UpdateCallCoordinatesAsync(boCall.Id, boCall.FullAddress);
    //    CallManager.SendEmailWhenCallOpenedAsync(boCall).GetAwaiter().GetResult();
    //    CallManager.Observers.NotifyListUpdated(); // stage 5
    //}
    public void AddCall(BO.Call boCall)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // stage 7

        var (lat, lon, error) = Task.Run(() => Tools.GetCoordinatesFromAddressAsync(boCall.FullAddress)).Result;

        if (lat == null || lon == null || lat == 0 || lon == 0)
            throw new BO.BlInvalidFormatException($"The address is invalid or could not be located on the map. {error}");

        boCall.Latitude = lat;
        boCall.Longitude = lon;

        DO.Call doCall;
        lock (AdminManager.BlMutex)
        {
            var existingCall = _dal.Call.Read(boCall.Id);
            if (existingCall != null)
                throw new BO.BlAlreadyExistException($"Call with ID={boCall.Id} already exist");
            CallManager.ValidateCall(boCall);
            doCall = CallManager.ConvertBoCallToDoCall(boCall);
            _dal.Call.Create(doCall);
        }
        CallManager.SendEmailWhenCallOpenedAsync(boCall).GetAwaiter().GetResult();
        CallManager.Observers.NotifyListUpdated(); // stage 5
    }


    /// <summary>
    /// Updates the details of an existing call.
    /// </summary>
    /// <param name="boCall">The BO.Call object containing the updated call information.</param>
    public void UpdateCallDetails(BO.Call boCall)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // stage 7
        DO.Call updatedCall;
        lock (AdminManager.BlMutex)
        {
            var existingCall = _dal.Call.Read(boCall.Id) ?? throw new BO.BlDoesNotExistException($"Call with ID={boCall.Id} does not exist");
            CallManager.ValidateCall(boCall);
            boCall.Latitude = null;
            boCall.Longitude = null;
            updatedCall = CallManager.ConvertBoCallToDoCall(boCall);
            _dal.Call.Update(updatedCall);
        }
        _ = UpdateCallCoordinatesAsync(boCall.Id, boCall.FullAddress);
        CallManager.Observers.NotifyItemUpdated(boCall.Id);  // stage 5
        CallManager.Observers.NotifyListUpdated(); // stage 5
    }

    /// <summary>
    /// Deletes a call from the system by its ID.
    /// </summary>
    /// <param name="callId">The ID of the call to delete.</param>
    public void DeleteCall(int callId)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // stage 7
        bool deleted = false;
        lock (AdminManager.BlMutex)
        {
            try
            {
                DO.Call call = _dal.Call.Read(callId) ?? throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist");
                if (_dal.Assignment.ReadAll(a => a!.CallId == callId).Any() || !(CallManager.CalculateCallStatus(call) == BO.Enums.CallStatus.opened))
                    throw new BO.BlDeletionException($"Cannot delete volunteer with ID={callId} as they are handling calls.");
                _dal.Call.Delete(callId);
                deleted = true;
            }
            catch (DO.DalDoesNotExistException ex)
            {
                throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist", ex);
            }
            catch (DO.DalDeletionImpossible ex)
            {
                throw new BO.BlDeletionException($"Cannot delete volunteer with ID={callId} as they are handling calls.", ex);
            }
            catch (Exception ex)
            {
                throw new BO.BlGeneralException(ex.Message, ex);
            }
        }
        if (deleted)
            CallManager.Observers.NotifyListUpdated(); // stage 5
    }

    /// <summary>
    /// Retrieves a list of closed calls handled by a specific volunteer.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer.</param>
    /// <param name="callTypeFilter">Optional filter for the type of calls.</param>
    /// <param name="sortField">Optional field by which to sort the list of calls.</param>
    /// <returns>A list of closed calls handled by the volunteer.</returns>
    public IEnumerable<BO.ClosedCallInList> GetClosedCallsHandledByVolunteer(int volunteerId, BO.Enums.CallType? callTypeFilter = null, BO.Enums.ClosedCallInListFields? sortField = null)
    {
        try
        {
            IEnumerable<BO.ClosedCallInList> closedCalls;
            lock (AdminManager.BlMutex)
            {
                closedCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == volunteerId && a.EndTimeForTreatment != null)
                    .Where(a => callTypeFilter is null || (BO.Enums.CallType)_dal.Call.Read(a.CallId)!.TypeOfCall == callTypeFilter)
                    .Select(a =>
                    {
                        var call = _dal.Call.Read(a.CallId);
                        return new BO.ClosedCallInList
                        {
                            Id = call!.ID,
                            CallType = (BO.Enums.CallType)call.TypeOfCall,
                            FullAddress = call.Address,
                            Opening_time = call.OpeningTime,
                            Start_time = a.EntryTimeForTreatment,
                            End_time = a.EndTimeForTreatment,
                            EndType = (BO.Enums.EndType)a.TypeOfFinishTreatment!
                        };
                    })
                    .ToList();
            }
            return sortField.HasValue
                ? closedCalls.OrderBy(a => a.GetType().GetProperty(sortField.ToString()!)?.GetValue(a))
                : closedCalls.OrderBy(a => a.Id);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistException("Cannot access assignments", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Retrieves a list of open calls for a specific volunteer.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer.</param>
    /// <param name="callTypeFilter">Optional filter for the type of calls.</param>
    /// <param name="sortField">Optional field by which to sort the list of calls.</param>
    /// <returns>A list of open calls for the volunteer.</returns>
    public async Task<IEnumerable<BO.OpenCallInList>> GetOpenCallsForVolunteerAsync(int volunteerId, BO.Enums.CallType? callTypeFilter = null, BO.Enums.OpenCallInListFields? sortField = null)
    {
        try
        {
            DO.Volunteer volunteer;
            BO.Enums.DistanceTypes volunteerDistanceType;
            List<DO.Call> calls;
            lock (AdminManager.BlMutex)
            {
                volunteer = _dal.Volunteer.Read(volunteerId) ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={volunteerId} does not exist.");
                volunteerDistanceType = (BO.Enums.DistanceTypes)volunteer.DistanceType;
                calls = _dal.Call.ReadAll()
                    .Where(c =>
                        (CallManager.CalculateCallStatus(c) == BO.Enums.CallStatus.opened || CallManager.CalculateCallStatus(c) == BO.Enums.CallStatus.opened_at_risk) &&
                        (callTypeFilter == null || (BO.Enums.CallType)c.TypeOfCall == callTypeFilter))
                    .ToList();
            }

            var callTasks = calls.Select(async c =>
            {
                double distance = await Tools.CalculateDistanceAsync(
                    volunteerDistanceType,
                    volunteer.Latitude ?? 0.0,
                    volunteer.Longitude ?? 0.0,
                    c.Latitude,
                    c.Longitude
                );

                return new BO.OpenCallInList
                {
                    Id = c.ID,
                    CallType = (BO.Enums.CallType)c.TypeOfCall,
                    Verbal_description = c.CallDescription,
                    FullAddress = c.Address,
                    Start_time = c.OpeningTime,
                    Max_finish_time = c.MaxTimeForClosing,
                    CallDistance = distance
                };
            });

            var result = await Task.WhenAll(callTasks);

            return sortField.HasValue
                ? result.OrderBy(c => c.GetType().GetProperty(sortField.ToString()!)?.GetValue(c))
                : result.OrderBy(c => c.Id);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Volunteer with ID={volunteerId} does not exist.", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException(ex.Message, ex);
        }
    }

    public void MarkCallCancellation(int volunteerId, int assignmentId, bool fromSimulator = false)
    {
        if (!fromSimulator)
            AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        DO.Assignment assignment;
        DO.Volunteer volunteer;
        DO.Assignment updatedAssignment;
        lock (AdminManager.BlMutex)
        {
            assignment = _dal.Assignment.Read(assignmentId)
                ?? throw new BO.BlDoesNotExistException($"Assignment with ID={assignmentId} does not exist.");

            volunteer = _dal.Volunteer.Read(volunteerId)
                ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={volunteerId} does not exist.");

            if (assignment.VolunteerId != volunteerId && (BO.Enums.Role)volunteer.Role != BO.Enums.Role.manager)
            {
                throw new BO.BlUnauthorizedException("Requester does not have permission to cancel this treatment.");
            }

            updatedAssignment = assignment with
            {
                EndTimeForTreatment = DateTime.Now,
                TypeOfFinishTreatment = DO.TypeOfFinishTreatment.SelfCancellation
            };

            _dal.Assignment.Update(updatedAssignment);
        }
        CallManager.SendEmailToVolunteerAsync(volunteer, assignment);
        CallManager.Observers.NotifyItemUpdated(updatedAssignment.ID);  // stage 5
        CallManager.Observers.NotifyListUpdated(); // stage 5
        VolunteerManager.Observers.NotifyItemUpdated(volunteerId);
        CallManager.Observers.NotifyItemUpdated(assignment.CallId);
        VolunteerManager.Observers.NotifyListUpdated();
    }

    /// <summary>
    /// Marks an assignment as complited for a volunteer.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer compliting the assignment.</param>
    /// <param name="assignmentId">The ID of the assignment to complite.</param>
    public void MarkCallCompletion(int volunteerId, int assignmentId, bool fromSimulator = false)
    {
        if (!fromSimulator)
            AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        DO.Assignment assignment;
        DO.Assignment newAssignment;
        lock (AdminManager.BlMutex)
        {
            assignment = _dal.Assignment.Read(assignmentId) ?? throw new BO.BlDoesNotExistException($"Assignment with ID={assignmentId} does not exist.");
            if (assignment.VolunteerId != volunteerId)
                throw new BO.BlUnauthorizedException($"Volunteer with ID={volunteerId} is not authorized to complete this call.");
            if (assignment.EndTimeForTreatment != null)
                throw new BO.BlDeletionException($"The assignment with ID={assignmentId} has already been completed or canceled.");
            newAssignment = assignment with { EndTimeForTreatment = _dal.Config.Clock, TypeOfFinishTreatment = DO.TypeOfFinishTreatment.Treated };
            _dal.Assignment.Update(newAssignment);
        }
        CallManager.Observers.NotifyItemUpdated(newAssignment.ID);  //stage 5
        CallManager.Observers.NotifyListUpdated(); //stage 5  
        VolunteerManager.Observers.NotifyItemUpdated(volunteerId);
        CallManager.Observers.NotifyItemUpdated(assignment.CallId);
        VolunteerManager.Observers.NotifyListUpdated();
    }

    /// <summary>
    /// Retrieves the quantities of calls by their status.
    /// </summary>
    /// <returns>An array of integers representing the quantities of calls for each status in the system.</returns>
    public int[] GetCallQuantitiesByStatus()
    {
        try
        {
            IEnumerable<DO.Call> calls;
            lock (AdminManager.BlMutex)
            {
                calls = _dal.Call.ReadAll();
            }
            int[] callQuantities = new int[Enum.GetValues(typeof(BO.Enums.CallStatus)).Length];

            var groupedCalls = calls
                .GroupBy(c => c.CalculateCallStatus())
                .ToDictionary(g => g.Key, g => g.Count());
            foreach (var entry in groupedCalls)
            {
                callQuantities[(int)entry.Key] = entry.Value;
            }
            return callQuantities;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Cannot access calls", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Retrieves a list of calls, optionally filtered and sorted by specified fields.
    /// </summary>
    /// <param name="fieldFilter">Optional filter field for the calls.</param>
    /// <param name="filterValue">The value to filter the calls by.</param>
    /// <param name="sortField">Optional field to sort the calls by.</param>
    /// <returns>An IEnumerable of calls that match the filter and sort criteria.</returns>
    public IEnumerable<BO.CallInList> GetCallsList(BO.Enums.CallInListFields? fieldFilter = null, object? filterValue = null, BO.Enums.CallInListFields? sortField = null)
    {
        try
        {
            IEnumerable<BO.CallInList?> calls;
            lock (AdminManager.BlMutex)
            {
                calls = CallManager.ConvertToCallInList((IEnumerable<DO.Call>)_dal.Call.ReadAll());
            }
            if (fieldFilter.HasValue && filterValue is not null)
            {
                calls = calls.Where(c => c!.GetType().GetProperty(fieldFilter.ToString()!)!.GetValue(c)?.Equals(filterValue) == true);
            }

            calls = sortField.HasValue
                ? calls.OrderBy(c => c?.GetType().GetProperty(sortField.ToString()!)?.GetValue(c))
                : calls.OrderBy(c => c?.CallId);
            return calls!;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Can not access calls", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Assigns a volunteer to a call for treatment, provided the call is eligible for treatment.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer who is assigning themselves to the call.</param>
    /// <param name="callId">The ID of the call to assign to the volunteer.</param>
    public void SelectCallForTreatment(int volunteerId, int callId ,bool fromSimulator = false)
    {
        if (!fromSimulator)
            AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        IEnumerable<DO.Assignment> existingAssignments;
        lock (AdminManager.BlMutex)
        {
            var call = GetCallDetails(callId) ?? throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist.");
            if (call.CallStatus == BO.Enums.CallStatus.is_treated || !(call.CallStatus == BO.Enums.CallStatus.opened))
                throw new BO.BlUnauthorizedException($"Call with ID={callId} is open already.You are not authorized to treat it.");
            if (call.Max_finish_time < DateTime.Now)
                throw new BO.BlUnauthorizedException($"Call with ID={callId} is expired already.You are not authorized to treat it.");
            existingAssignments = _dal.Assignment.ReadAll(a => a?.CallId == callId);
            if (existingAssignments.Any(a => a?.EndTimeForTreatment == null))
                throw new BO.BlUnauthorizedException($"Call with ID={callId} is in treatment already.You are not authorized to treat it.");
            var newAssignment = new DO.Assignment
            {
                CallId = callId,
                VolunteerId = volunteerId,
                EntryTimeForTreatment = _dal.Config.Clock,
                EndTimeForTreatment = null,
                TypeOfFinishTreatment = null
            };
            _dal.Assignment.Create(newAssignment);
        }
        VolunteerManager.Observers.NotifyItemUpdated(volunteerId);
        CallManager.Observers.NotifyItemUpdated(callId);
        CallManager.Observers.NotifyListUpdated(); //stage 5  
        VolunteerManager.Observers.NotifyListUpdated();
    }
    private async Task UpdateCallCoordinatesAsync(int callId, string address)
    {
        var (latitude, longitude, error) = await Tools.GetCoordinatesFromAddressAsync(address);

        if (latitude == 0 || longitude == 0)
        {            return;
        }
        //if (latitude != null && longitude != null)
        //{
        lock (AdminManager.BlMutex)
            {
                var call = _dal.Call.Read(callId);
                if (call != null)
                {
                    var updatedCall = call with { Latitude = latitude.Value, Longitude = longitude.Value };
                    _dal.Call.Update(updatedCall);
                }
            }
            CallManager.Observers.NotifyItemUpdated(callId);
        
    }

    #region Stage 5
    public void AddObserver(Action listObserver) =>
        CallManager.Observers.AddListObserver(listObserver); //stage 5

    public void AddObserver(int id, Action observer) =>
        CallManager.Observers.AddObserver(id, observer); //stage 5

    public void RemoveObserver(Action listObserver) =>
        CallManager.Observers.RemoveListObserver(listObserver); //stage 5

    public void RemoveObserver(int id, Action observer) =>
        CallManager.Observers.RemoveObserver(id, observer); //stage 5
    #endregion Stage 5
}
