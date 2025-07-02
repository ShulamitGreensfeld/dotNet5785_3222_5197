using BlApi;
using BO;
using DO;
using Helpers;
using System.Net;

namespace BlImplementation;

internal class VolunteerImplementation : IVolunteer
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    public BO.Volunteer GetVolunteerDetails(int volunteerId)
    {
        try
        {
            DO.Volunteer volunteerDO;
            DO.Assignment? currentAssignment;
            IEnumerable<DO.Assignment> assignments;

            lock (AdminManager.BlMutex)
            {
                volunteerDO = _dal.Volunteer.Read(volunteerId) ??
                    throw new BO.BlNotFoundException($"Volunteer with ID={volunteerId} does not exist.");

                currentAssignment = _dal.Assignment.ReadAll(a => a.VolunteerId == volunteerId && a.EndTimeForTreatment == null).FirstOrDefault();

                assignments = _dal.Assignment.ReadAll(a => a.VolunteerId == volunteerId);
            }

            BO.Enums.DistanceTypes volunteerDistanceType = (BO.Enums.DistanceTypes)volunteerDO.DistanceType;

            BO.CallInProgress? callInProgress = null;
            if (currentAssignment != null)
            {
                DO.Call? callDetails;
                lock (AdminManager.BlMutex)
                {
                    callDetails = _dal.Call.Read(currentAssignment.CallId);
                }
                if (callDetails != null)
                {
                    callInProgress = new BO.CallInProgress
                    {
                        Id = currentAssignment.ID,
                        CallId = currentAssignment.CallId,
                        CallType = (BO.Enums.CallType)callDetails.TypeOfCall,
                        Verbal_description = callDetails.CallDescription,
                        FullAddress = callDetails.Address,
                        Opening_time = callDetails.OpeningTime,
                        Max_finish_time = callDetails.MaxTimeForClosing ?? throw new InvalidOperationException("MaxTimeForClosing cannot be null"),
                        Start_time = currentAssignment.EntryTimeForTreatment,
                        CallDistance = Tools.CalculateDistanceAsync(
                            volunteerDistanceType,
                            volunteerDO.Latitude ?? 0,
                            volunteerDO.Longitude ?? 0,
                            callDetails.Latitude,
                            callDetails.Longitude).Result,
                        CallStatus = CalculateStatus(callDetails, 30)
                    };
                }
            }

            int totalHandled = assignments.Count(a => a.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.Treated);
            int totalCanceled = assignments.Count(a =>
                a.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.SelfCancellation ||
                a.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.ManagerCancellation);
            int totalExpired = assignments.Count(a => a.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.OutOfRangeCancellation);

            return new BO.Volunteer
            {
                Id = volunteerDO.ID,
                FullName = volunteerDO.Name,
                CellphoneNumber = volunteerDO.Phone,
                Email = volunteerDO.Email,
                Password = volunteerDO.Password,
                FullAddress = volunteerDO.Address,
                Latitude = volunteerDO.Latitude,
                Longitude = volunteerDO.Longitude,
                Role = (BO.Enums.Role)volunteerDO.Role,
                IsActive = volunteerDO.IsActive,
                MaxDistance = volunteerDO.MaxDistanceForCall,
                DistanceType = (BO.Enums.DistanceTypes)volunteerDO.DistanceType,
                CallInProgress = callInProgress,
                TotalHandledCalls = totalHandled,
                TotalCanceledCalls = totalCanceled,
                TotalExpiredCalls = totalExpired
            };
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlNotFoundException("Volunteer not found in data layer.", ex);
        }
    }

    private BO.Enums.CallStatus CalculateStatus(DO.Call callDetails, int threshold)
    {
        return BO.Enums.CallStatus.opened;
    }

    public IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActiveFilter = null, BO.Enums.VolunteerInListFields? fieldSort = null)
    {
        try
        {
            IEnumerable<DO.Volunteer> volunteers;
            lock (AdminManager.BlMutex)
            {
                volunteers = _dal.Volunteer.ReadAll();
            }

            if (isActiveFilter is not null)
                volunteers = volunteers.Where(v => v?.IsActive == isActiveFilter.Value);

            var allVolunteersInList = VolunteerManager.GetVolunteerList(volunteers!);

            var sortedVolunteers = fieldSort is not null ? fieldSort switch
            {
                BO.Enums.VolunteerInListFields.FullName => allVolunteersInList.OrderBy(v => v?.FullName).ToList(),
                BO.Enums.VolunteerInListFields.TotalHandledCalls => allVolunteersInList.OrderBy(v => v?.TotalHandledCalls).ToList(),
                BO.Enums.VolunteerInListFields.TotalCanceledCalls => allVolunteersInList.OrderBy(v => v?.TotalCanceledCalls).ToList(),
                BO.Enums.VolunteerInListFields.TotalExpiredCalls => allVolunteersInList.OrderBy(v => v?.TotalExpiredCalls).ToList(),
                BO.Enums.VolunteerInListFields.CallType => allVolunteersInList.OrderBy(v => v?.CallType).ToList(),
                _ => allVolunteersInList.OrderBy(v => v?.Id).ToList(),
            } : allVolunteersInList.OrderBy(v => v?.Id).ToList();

            return sortedVolunteers;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Error accessing Volunteers.", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException(ex.Message, ex);
        }
    }

    public void AddVolunteer(BO.Volunteer boVolunteer)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // stage 7
        try
        {
            lock (AdminManager.BlMutex)
            {
                if (_dal.Volunteer.Read(boVolunteer.Id) is not null)
                    throw new BO.BlDoesNotExistException($"Volunteer with ID={boVolunteer.Id} already exists");
            }

            VolunteerManager.ValidateVolunteer(boVolunteer);

            if (string.IsNullOrEmpty(boVolunteer.Password))
            {
                boVolunteer.Password = VolunteerManager.GenerateStrongPassword();
            }
            boVolunteer.Password = VolunteerManager.EncryptPassword(boVolunteer.Password);

            // שמירה ראשונית עם קואורדינטות null
            boVolunteer.Latitude = null;
            boVolunteer.Longitude = null;

            DO.Volunteer doVolunteer = VolunteerManager.ConvertBoVolunteerToDoVolunteer(boVolunteer);
            lock (AdminManager.BlMutex)
            {
                _dal.Volunteer.Create(doVolunteer);
            }
            VolunteerManager.Observers.NotifyListUpdated(); //stage 5

            if (!string.IsNullOrEmpty(boVolunteer.FullAddress))
            {
                _ = UpdateVolunteerCoordinatesAsync(boVolunteer.Id, boVolunteer.FullAddress);
            }
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistException($"Volunteer with ID={boVolunteer.Id} already exists", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException(ex.Message, ex);
        }
    }

    private async Task UpdateVolunteerCoordinatesAsync(int volunteerId, string address)
    {
        (double? latitude, double? longitude, string? error) = await Tools.GetCoordinatesFromAddressAsync(address);
        if (latitude != null && longitude != null)
        {
            lock (AdminManager.BlMutex)
            {
                var volunteer = _dal.Volunteer.Read(volunteerId);
                if (volunteer != null)
                {
                    var updatedVolunteer = volunteer with { Latitude = latitude, Longitude = longitude };
                    _dal.Volunteer.Update(updatedVolunteer);
                }
            }
            VolunteerManager.Observers.NotifyItemUpdated(volunteerId);
        }
    }

    /// <summary>
    /// Updates the details of a volunteer, including address and password validation.
    /// </summary>
    /// <param name="requesterId">The ID of the requester trying to update the volunteer's details.</param>
    /// <param name="boVolunteer">A BO.Volunteer object with the updated volunteer details.</param>
    /// <summary>
    /// Updates the details of a volunteer, including address and password validation.
    /// </summary>
    /// <param name="requesterId">The ID of the requester trying to update the volunteer's details.</param>
    /// <param name="boVolunteer">A BO.Volunteer object with the updated volunteer details.</param>
    public void UpdateVolunteerDetails(int requesterId, BO.Volunteer boVolunteer)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // stage 7

        DO.Volunteer updatedVolunteer;

        lock (AdminManager.BlMutex)
        {
            try
            {
                DO.Volunteer requester = _dal.Volunteer.Read(requesterId)
                    ?? throw new BO.BlDoesNotExistException("Requester does not exist!");

                if (requester.ID != boVolunteer.Id && requester.Role != DO.Role.Manager)
                    throw new BO.BlUnauthorizedException("Requester is not authorized!");

                DO.Volunteer existingVolunteer = _dal.Volunteer.Read(boVolunteer.Id)
                    ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={boVolunteer.Id} does not exist");

                VolunteerManager.ValidateVolunteer(boVolunteer);

                if (!string.IsNullOrWhiteSpace(boVolunteer.Password) && boVolunteer.Password != existingVolunteer.Password)
                {
                    if (!VolunteerManager.IsPasswordStrong(boVolunteer.Password))
                        throw new BO.BlInvalidFormatException("Password is not strong!");
                    boVolunteer.Password = VolunteerManager.EncryptPassword(boVolunteer.Password);
                }
                else
                {
                    boVolunteer.Password = existingVolunteer.Password;
                }
                if (requester.Role != DO.Role.Manager && requester.Role != (DO.Role)boVolunteer.Role)
                    throw new BO.BlUnauthorizedException("Requester is not authorized to change the Role field!");
                if (!string.IsNullOrWhiteSpace(boVolunteer.FullAddress))
                {
                    boVolunteer.Latitude = null;
                    boVolunteer.Longitude = null;
                }
                updatedVolunteer = VolunteerManager.ConvertBoVolunteerToDoVolunteer(boVolunteer);
                _dal.Volunteer.Update(updatedVolunteer);
            }
            catch (DO.DalDoesNotExistException ex)
            {
                throw new BO.BlDoesNotExistException($"Volunteer with ID={boVolunteer.Id} does not exist", ex);
            }
            catch (BO.BlInvalidFormatException ex)
            {
                throw new BO.BlInvalidFormatException($"Invalid format for volunteer details: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new BO.BlGeneralException(ex.Message, ex);
            }
        }
        if (!string.IsNullOrWhiteSpace(boVolunteer.FullAddress))
        {
            _ = UpdateVolunteerCoordinatesAsync(boVolunteer.Id, boVolunteer.FullAddress);
        }

        // Notifications רק מחוץ ל-lock
        VolunteerManager.Observers.NotifyItemUpdated(updatedVolunteer.ID);  //stage 5
        VolunteerManager.Observers.NotifyListUpdated(); //stage 5
    }

    /// <summary>
    /// Deletes a volunteer from the system if they are not handling any active calls.
    /// </summary>
    /// <param name="id">The ID of the volunteer to delete.</param>
    public void DeleteVolunteer(int id)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // stage 7
        bool deleted = false;

        lock (AdminManager.BlMutex)
        {
            try
            {
                var volunteer = _dal.Volunteer.Read(id) ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={id} does not exist");
                if (_dal.Assignment.ReadAll(a => a!.VolunteerId == id).Any())
                    throw new BO.BlDeletionException($"Cannot delete volunteer with ID={id} as they are handling calls.");
                _dal.Volunteer.Delete(id);
                deleted = true;
            }
            catch (DO.DalDoesNotExistException ex)
            {
                throw new BO.BlDoesNotExistException($"Volunteer with ID={id} does not exist", ex);
            }
            catch (Exception ex)
            {
                throw new BO.BlGeneralException(ex.Message, ex);
            }
        }

        if (deleted)
        {
            VolunteerManager.Observers.NotifyListUpdated(); //stage 5
        }
    }

    /// <summary>
    /// Allows a volunteer to log in by verifying their username and password.
    /// </summary>
    /// <param name="name">The name of the volunteer.</param>
    /// <param name="pass">The password of the volunteer.</param>
    /// <returns>The role of the volunteer if login is successful.</returns>
    private static readonly HashSet<int> _loggedInVolunteers = new();
    private static int? _managerId = null;

    public BO.Enums.Role EnterSystem(string id, string pass)
    {
        try
        {
            // Validate ID format
            if (string.IsNullOrWhiteSpace(id))
                throw new BO.BlInvalidFormatException("User ID is required.");

            if (!int.TryParse(id, out int volunteerId))
                throw new BO.BlInvalidFormatException("Invalid ID format. Must be a number.");

            // Validate password presence
            if (string.IsNullOrWhiteSpace(pass))
                throw new BO.BlInvalidFormatException("Password is required.");

            // Attempt to find the volunteer - צריך להחזיר DO ואז להמיר ל-BO
            DO.Volunteer doVolunteer;
            lock (AdminManager.BlMutex)
            {
                doVolunteer = _dal.Volunteer.ReadAll()
                    .FirstOrDefault(v => v!.ID == volunteerId && VolunteerManager.VerifyPassword(pass, v.Password!));
            }

            if (doVolunteer == null)
                throw new BO.BlUnauthorizedException("Invalid ID or password.");

            BO.Volunteer volunteer = VolunteerManager.ConvertDoVolunteerToBoVolunteer(doVolunteer).GetAwaiter().GetResult();
            // Prevent duplicate login
            lock (_loggedInVolunteers)
            {
                if (_loggedInVolunteers.Contains(volunteerId)&& doVolunteer.Role != Role.Volunteer)
                    throw new BO.BlUnauthorizedException("You are already logged in.");

                if (volunteer.Role == BO.Enums.Role.manager)
                {
                    if (_managerId == null)
                    {
                        _managerId = volunteerId;
                        return BO.Enums.Role.manager;
                    }
                    else if (_managerId == volunteerId)
                    {
                        if (_loggedInVolunteers.Contains(volunteerId))
                            throw new BO.BlUnauthorizedException("You are already logged in twice.");

                        _loggedInVolunteers.Add(volunteerId);
                        return BO.Enums.Role.volunteer;
                    }
                    else
                    {
                        _loggedInVolunteers.Add(volunteerId);
                        return BO.Enums.Role.volunteer;
                    }
                }

                _loggedInVolunteers.Add(volunteerId);
                return BO.Enums.Role.volunteer;
            }
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Error accessing volunteers.", ex);
        }
        catch (BO.BlInvalidFormatException)
        {
            throw; // Allow PL to display message
        }
        catch (BO.BlUnauthorizedException)
        {
            throw; // Allow PL to display message
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException("An unexpected error occurred during login.", ex);
        }
    }
    /// <summary>
    /// Retrieves a list of volunteers, optionally filtered by call type and sorted by a specific field.
    /// </summary>
    /// <param name="callTypeFilter">Optional filter for the volunteer's call type.</param>
    /// <param name="fieldSort">Optional sorting field for the volunteers list.</param>
    /// <returns>A list of BO.VolunteerInList objects representing the volunteers.</returns>
    public IEnumerable<VolunteerInList> GetVolunteersFilterList(BO.Enums.CallType? callType) //stage 5
    {
        try
        {
            IEnumerable<VolunteerInList> volunteers;
            lock (AdminManager.BlMutex)
            {
                if (callType is null)
                    volunteers = GetVolunteersList();
                else
                    volunteers = GetVolunteersList().Where(v => v.CallType == callType);
            }
            return volunteers;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Error accessing Volunteers.", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException("An unexpected error occurred.", ex);
        }
    }
    public void LogoutVolunteer(int id)
    {
        lock (_loggedInVolunteers)
        {
            _loggedInVolunteers.Remove(id);
        }
    }

    #region Stage 5
    public void AddObserver(Action listObserver) =>
        VolunteerManager.Observers.AddListObserver(listObserver); //stage 5

    public void AddObserver(int id, Action observer) =>
        VolunteerManager.Observers.AddObserver(id, observer); //stage 5

    public void RemoveObserver(Action listObserver) =>
        VolunteerManager.Observers.RemoveListObserver(listObserver); //stage 5

    public void RemoveObserver(int id, Action observer) =>
        VolunteerManager.Observers.RemoveObserver(id, observer); //stage 5
    #endregion Stage 5
}