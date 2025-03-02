using Helpers;
using System.Collections.Generic;
using System.Linq;

//﻿using Helpers;
namespace BlImplementation;

internal class CallImplementation : BlApi.ICall
{

    private readonly DalApi.IDal _dal = DalApi.Factory.Get;
    private bool RequesterVolunteer;

    public int[] GetCallQuantitiesByStatus()
    {
        try
        {
            var calls = _dal.Call.ReadAll();
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
            throw new BO.BlGeneralException("Unexpected error occurred.", ex);
        }
    }

    public IEnumerable<BO.CallInList> GetCallsList(BO.Enums.CallInListFields? fieldFilter = null, object? filterValue = null, BO.Enums.CallInListFields? sortField = null)
    {
        try
        {
            IEnumerable<BO.CallInList?> calls = CallManager.ConvertToCallInList((IEnumerable<DO.Call>)_dal.Call.ReadAll());
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
            throw new BO.BlGeneralException("Unexpected error occurred.", ex);
        }
    }

    public BO.Call GetCallDetails(int callId)
    {
        try
        {
            DO.Call call = _dal.Call.Read(callId) ?? throw new BO.BlDoesNotExistException($"Student with ID={callId} does Not exist");
            var callAssignInLists = _dal.Assignment.ReadAll(a => a.CallId == callId)
                                               .Select(a => new BO.CallAssignInList
                                               {
                                                   VolunteerId = a.VolunteerId,
                                                   VolunteerFullName = _dal.Volunteer.Read(a.VolunteerId)!.Name,
                                                   Start_time = a.EntryTimeForTreatment,
                                                   End_time = a.EndTimeForTreatment,
                                                   EndType = (BO.Enums.EndType?)a.TypeOfFinishTreatment
                                               })
                                               .ToList();
            return new BO.Call
            {
                Id = call.ID,
                CallType = (BO.Enums.CallType)call.TypeOfCall,
                //Verbal_description = call.Verbal_description,  //לבדוק למה צריך את המשתנה הזה בכלל.....
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
            throw new BO.BlGeneralException("Unexpected error occurred.", ex);
        }
    }
    public void UpdateCallDetails(BO.Call boCall)
    {
        try
        {
            var existingCall = _dal.Call.Read(boCall.Id) ?? throw new BO.BlDoesNotExistException($"Call with ID={boCall.Id} does not exist");
            CallManager.ValidateCall(boCall);
            var (latitude, longitude) = Tools.GetCoordinatesFromAddress(boCall.FullAddress!);
            if (latitude is null || longitude is null)
                throw new BO.BlInvalidFormatException($"Invalid address: {boCall.FullAddress}");
            boCall.Latitude = latitude;
            boCall.Longitude = longitude;
            DO.Call updatedCall = CallManager.ConvertBoCallToDoCall(boCall);
            _dal.Call.Update(updatedCall);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Can not access calls", ex);
        }
           catch (Exception ex)
        {
            throw new BO.BlGeneralException("Unexpected error occurred.", ex);
        }
    }

    public void DeleteCall(int callId)
    {
        try
        {
            DO.Call call = _dal.Call.Read(callId) ?? throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist");
            if (!_dal.Assignment.ReadAll(a => a!.CallId == callId).Any() || CallManager.CalculateCallStatus(call) != BO.Enums.CallStatus.opened)
                throw new BO.BlDeletionException($"Cannot delete volunteer with ID={callId} as they are handling calls.");
            _dal.Call.Delete(callId);
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
            throw new BO.BlGeneralException("Unexpected error occurred.", ex);
        }
    }


    public void AddCall(BO.Call boCall)
    {
        try
        {
            var existingCall = _dal.Call.Read(boCall.Id);
            if (existingCall != null)
                throw new BO.BlAlreadyExistException($"Call with ID={boCall.Id} already exist");
            CallManager.ValidateCall(boCall);
            var (latitude, longitude) = Tools.GetCoordinatesFromAddress(boCall.FullAddress!);
            if (latitude is null || longitude is null)
                throw new BO.BlInvalidFormatException($"Invalid address: {boCall.FullAddress}");
            boCall.Latitude = latitude;
            boCall.Longitude = longitude;
            DO.Call doCall = CallManager.ConvertBoCallToDoCall(boCall);
            _dal.Call.Create(doCall);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistException($"Call with ID={boCall.Id} already exists", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException("Unexpected error occurred.", ex);
        }
    }

    public IEnumerable<BO.ClosedCallInList> GetClosedCallsHandledByVolunteer(int volunteerId, BO.Enums.CallType? callTypeFilter = null, BO.Enums.ClosedCallInListFields? sortField = null)
    {
        try
        {
            var closedCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == volunteerId && a.EndTimeForTreatment != null)
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
                     });
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
            throw new BO.BlGeneralException("Unexpected error occurred.", ex);
        }
    }

    public IEnumerable<BO.OpenCallInList> GetOpenCallsForVolunteer(int volunteerId, BO.Enums.CallType? callTypeFilter = null, BO.Enums.OpenCallInListFields? sortField = null)
    {
        try
        {
            var volunteer = _dal.Volunteer.Read(volunteerId) ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={volunteerId} does not exist.");
            var openCalls = _dal.Call.ReadAll()
                .Where(c =>
                (CallManager.CalculateCallStatus(c) == BO.Enums.CallStatus.opened || CallManager.CalculateCallStatus(c) == BO.Enums.CallStatus.opened_at_risk))
                .Select(c => new BO.OpenCallInList
                {
                    Id = volunteerId,
                    CallType = (BO.Enums.CallType)c.TypeOfCall,
                    Verbal_description = c.CallDescription,
                    FullAddress = c.Address,
                    Start_time = c.OpeningTime,
                    Max_finish_time = c.MaxTimeForClosing,
                    CallDistance = Tools.CalculateDistance(volunteer.Latitude, volunteer.Longitude, c.Latitude, c.Longitude)
                });
            return sortField.HasValue
            ? openCalls.OrderBy(c => c.GetType().GetProperty(sortField.ToString()!)?.GetValue(c))
            : openCalls.OrderBy(c => c.Id);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Volunteer with ID={volunteerId} does not exist.", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException("Unexpected error occurred.", ex);
        }
    }
    public void MarkCallCancellation(int volunteerId, int assignmentId)
    {
        try
        {
            var assignment = _dal.Assignment.Read(assignmentId);
            if (assignment == null)
                throw new BO.BlDoesNotExistException($"Assignment with ID={assignmentId} does not exist");
            var isRequesterManager = _dal.Volunteer?.Read(volunteerId)!.Role;
            if (isRequesterManager != DO.Role.Manager || assignment.VolunteerId != volunteerId)
                throw new BO.BlUnauthorizedException("Requester does not have permission to cancel this assignment");
            if (assignment.EndTimeForTreatment != null)
                throw new BO.BlDeletionException("Cannot cancel an assignment that has already been completed or expired");
            DO.Assignment copy = assignment with
            {
                EndTimeForTreatment = _dal.Config.Clock,
                TypeOfFinishTreatment = (DO.TypeOfFinishTreatment)(RequesterVolunteer ? BO.Enums.EndType.self_cancellation : BO.Enums.EndType.manager_cancellation),
            };
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Volunteer with ID={volunteerId} does not exist.", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException("Unexpected error occurred.", ex);
        }
    }




    public void MarkCallCompletion(int volunteerId, int assignmentId)
    {
        try
        {
            var assignment = _dal.Assignment.Read(assignmentId) ?? throw new BO.BlDoesNotExistException($"Assignment with ID={assignmentId} does not exist.");
            if (assignment.VolunteerId != volunteerId)
                throw new BO.BlUnauthorizedException($"Volunteer with ID={volunteerId} is not authorized to complete this call.");
            if (assignment.EndTimeForTreatment != null)
                throw new BO.BlDeletionException($"The assignment with ID={assignmentId} has already been completed or canceled.");
            DO.Assignment newAssignment = assignment with { EndTimeForTreatment = _dal.Config.Clock, TypeOfFinishTreatment = DO.TypeOfFinishTreatment.Treated };
            _dal.Assignment.Update(newAssignment);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Assignment with ID={assignmentId} does not exist", ex);
        }
        catch (DO.DalDeletionImpossible ex)
        {
            throw new BO.BlDeletionException($"The assignment with ID={assignmentId} has already been completed or canceled.", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException("Unexpected error occurred.", ex);
        }
    }


    public void SelectCallForTreatment(int volunteerId, int callId)
    {
        try
        {
            var call = GetCallDetails(callId) ?? throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist.");
            if (call.CallStatus == BO.Enums.CallStatus.is_treated || call.CallStatus == BO.Enums.CallStatus.opened)
                throw new BO.BlUnauthorizedException($"Call with ID={callId} is open already.You are not authorized to treat it.");
            if (call.Max_finish_time < DateTime.Now)
                throw new BO.BlUnauthorizedException($"Call with ID={callId} is expired already.You are not authorized to treat it.");
            var existingAssignments = _dal.Assignment.ReadAll(a => a?.CallId == callId);
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
            _dal.Assignment.Update(newAssignment);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException("Unexpected error occurred.", ex);
        }
    }


}






//public int[] GetCallQuantitiesByStatus()
//{
//    var calls = _dal.Call.ReadAll();

//    // יצירת מערך בגודל של מספר הסטטוסים (מומלץ שיהיה גודל של Enum)
//    int[] callQuantities = new int[Enum.GetValues(typeof(CallStatus)).Length];

//    // סינון הקריאות על פי סטטוס וחישוב הכמויות
//    var groupedCalls = calls
//        .GroupBy(c => c.CalculateCallStatus())  // קיבוץ לפי סטטוס
//        .ToArray();  // ממיר את הקבוצות למערך

//    foreach (var group in groupedCalls)
//    {
//        // עדכון המערך עם כמות הקריאות לכל סטטוס
//        callQuantities[(int)group.Key] = group.Count();
//    }





//if (fieldFilter is not null)
//    calls = calls.Where(call =>
//        fieldFilter.Value switch
//        {
//            BO.Enums.CallInListFields.CallId => call.CallId.Equals(filterValue),
//            BO.Enums.CallInListFields.CallType => call.CallType.Equals(filterValue),
//            BO.Enums.CallInListFields.Opening_time => call.Opening_time.Equals(filterValue),
//            BO.Enums.CallInListFields.TimeLeft => call.TimeLeft.Equals(filterValue),
//            BO.Enums.CallInListFields.LastVolunteerName => call.LastVolunteerName.Contains((string)filterValue),
//            BO.Enums.CallInListFields.TotalTime => call.TotalTime.Equals(filterValue),
//            BO.Enums.CallInListFields.CallStatus => call.CallStatus.Equals(filterValue),
//            BO.Enums.CallInListFields.TotalAssignments => call.TotalAssignments.Equals(filterValue),
//            _ => true
//        });
//if (sortField.HasValue)
//{
//    calls = calls.OrderBy(call =>
//        sortField.Value switch
//        {
//            BO.Enums.CallInListFields.CallId => call.CallId,
//            BO.Enums.CallInListFields.CallType => call.CallType,
//            BO.Enums.CallInListFields.Opening_time => call.Opening_time,
//            BO.Enums.CallInListFields.TimeLeft => call.TimeLeft,
//            BO.Enums.CallInListFields.LastVolunteerName => call.LastVolunteerName,
//            BO.Enums.CallInListFields.TotalTime => call.TotalTime,
//            BO.Enums.CallInListFields.CallStatus => call.CallStatus,
//            BO.Enums.CallInListFields.TotalAssignments => call.TotalAssignments,
//            _ => call.CallId
//        });
//}
//else
//{
//    calls = calls.OrderBy(call => call.CallId);
//}


//if (fieldFilter.HasValue && filterValue != null)
//{
//    calls = calls.Where(call =>
//        fieldFilter.Value switch
//        {
//            BO.Enums.CallFields.CallId => call.CallId.Equals(filterValue),
//            BO.Enums.CallFields.CallType => call.CallType.Equals(filterValue),
//            BO.Enums.CallFields.Opening_time => call.Opening_time.Equals(filterValue),
//            BO.Enums.CallFields.TimeLeft => call.TimeLeft.Equals(filterValue),
//            BO.Enums.CallFields.LastVolunteerName => call.LastVolunteerName.Contains((string)filterValue),
//            BO.Enums.CallFields.TotalTime => call.TotalTime.Equals(filterValue),
//            BO.Enums.CallFields.CallStatus => call.CallStatus.Equals(filterValue),
//            BO.Enums.CallFields.TotalAssignments => call.TotalAssignments.Equals(filterValue),
//            _ => true
//        });
//}