using BlApi;
using BO;
using Helpers;
using System.Net;

namespace BlImplementation;

internal class VolunteerImplementation : IVolunteer
{

    private readonly DalApi.IDal _dal = DalApi.Factory.Get;


    /// <summary>
    /// Retrieves the details of a volunteer by their ID.
    /// </summary>
    /// <param name="id">The ID of the volunteer.</param>
    /// <returns>A BO.Volunteer object representing the volunteer's details.</returns>
    public BO.Volunteer GetVolunteerDetails(int volunteerId)
    {
        try
        {
            var volunteerDO = _dal.Volunteer.Read(volunteerId) ??
                throw new BO.BlNotFoundException($"Volunteer with ID={volunteerId} does not exist.");

            var currentAssignment = _dal.Assignment.ReadAll(a => a.VolunteerId == volunteerId && a.EndTimeForTreatment == null).FirstOrDefault();

            BO.CallInProgress? callInProgress = null;
            if (currentAssignment != null)
            {
                var callDetails = _dal.Call.Read(currentAssignment.CallId);
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
                        CallDistance = Tools.CalculateDistance(volunteerDO.Latitude!, volunteerDO.Longitude!, callDetails.Latitude, callDetails.Longitude),
                        CallStatus = CalculateStatus(callDetails, 30)
                    };
                }
            }
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
                CallInProgress = callInProgress
            };
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlNotFoundException("Volunteer not found in data layer.", ex);
        }
    }


    private BO.Enums.CallStatus CalculateStatus(DO.Call callDetails, int threshold)
    {
        // Implement the logic to calculate the call status based on callDetails and threshold
        // This is a placeholder implementation
        return BO.Enums.CallStatus.opened;
    }
    /// <summary>
    /// Retrieves a list of volunteers, optionally filtered by activity status and sorted by a specific field.
    /// </summary>
    /// <param name="isActiveFilter">Optional filter for the volunteer's active status.</param>
    /// <param name="fieldSort">Optional sorting field for the volunteers list.</param>
    /// <returns>A list of BO.VolunteerInList objects representing the volunteers.</returns>
    public IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActiveFilter = null, BO.Enums.VolunteerInListFields? fieldSort = null)
    {
        try
        {
            var volunteers = _dal.Volunteer.ReadAll();
            if (isActiveFilter is not null)
                volunteers = volunteers.Where(v => v?.IsActive == isActiveFilter.Value);
            var allVolunteersInList = VolunteerManager.GetVolunteerList(volunteers!);
            var sortedVolunteers = fieldSort is not null ? fieldSort switch
            {
                BO.Enums.VolunteerInListFields.FullName => allVolunteersInList.OrderBy(v => v?.FullName).ToList(),
                BO.Enums.VolunteerInListFields.TotalHandledCalls => allVolunteersInList.OrderBy(v => v?.TotalHandledCalls).ToList(),
                BO.Enums.VolunteerInListFields.TotalCanceledCalls => allVolunteersInList.OrderBy(v => v?.TotalCanceledCalls).ToList(),
                BO.Enums.VolunteerInListFields.TotalExpiredCalls => allVolunteersInList.OrderBy(v => v?.TotalExpiredCalls).ToList(),
                //BO.Enums.VolunteerInListFields.CallId => allVolunteersInList.OrderBy(v => v?.TotalExpiredCalls).ToList(),
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

    /// <summary>
    /// Adds a new volunteer to the system after validating their information.
    /// </summary>
    /// <param name="boVolunteer">A BO.Volunteer object representing the volunteer to add.</param>
    public void AddVolunteer(BO.Volunteer boVolunteer)
    {
        try
        {
            if (_dal.Volunteer.Read(boVolunteer.Id) is not null)
                throw new BO.BlDoesNotExistException($"Volunteer with ID={boVolunteer.Id} already exists");

            VolunteerManager.ValidateVolunteer(boVolunteer);

            if (string.IsNullOrEmpty(boVolunteer.Password))
            {
                boVolunteer.Password = VolunteerManager.GenerateStrongPassword();
            }
            boVolunteer.Password = VolunteerManager.EncryptPassword(boVolunteer.Password);
            if (boVolunteer.FullAddress != null && boVolunteer.FullAddress != "")
            {
                var (latitude, longitude) = Tools.GetCoordinatesFromAddress(boVolunteer.FullAddress!);
                if (latitude == null || longitude == null)
                {
                    boVolunteer.FullAddress = null;
                    throw new BO.BlInvalidFormatException($"Invalid address: {boVolunteer.FullAddress}");
                }
                boVolunteer.Latitude = latitude;
                boVolunteer.Longitude = longitude;
            }
            else
                boVolunteer.FullAddress = null;

            // Convert BO to DO and save the volunteer
            DO.Volunteer doVolunteer = VolunteerManager.ConvertBoVolunteerToDoVolunteer(boVolunteer);
            _dal.Volunteer.Create(doVolunteer);
            VolunteerManager.Observers.NotifyListUpdated(); //stage 5                                                    
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

    /// <summary>
    /// Updates the details of a volunteer, including address and password validation.
    /// </summary>
    /// <param name="requesterId">The ID of the requester trying to update the volunteer's details.</param>
    /// <param name="boVolunteer">A BO.Volunteer object with the updated volunteer details.</param>
    public void UpdateVolunteerDetails(int requesterId, BO.Volunteer boVolunteer)
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
                var (latitude, longitude) = Tools.GetCoordinatesFromAddress(boVolunteer.FullAddress);
                if (latitude == null || longitude == null)
                    throw new BO.BlInvalidFormatException($"Invalid address: {boVolunteer.FullAddress}");

                boVolunteer.Latitude = latitude;
                boVolunteer.Longitude = longitude;
            }
            DO.Volunteer updatedVolunteer = VolunteerManager.ConvertBoVolunteerToDoVolunteer(boVolunteer);
            _dal.Volunteer.Update(updatedVolunteer);
            VolunteerManager.Observers.NotifyItemUpdated(updatedVolunteer.ID);  //stage 5
            VolunteerManager.Observers.NotifyListUpdated(); //stage 5
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


    /// <summary>
    /// Deletes a volunteer from the system if they are not handling any active calls.
    /// </summary>
    /// <param name="id">The ID of the volunteer to delete.</param>
    public void DeleteVolunteer(int id)
    {
        try
        {
            var volunteer = _dal.Volunteer.Read(id) ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={id} does not exist");
            if (_dal.Assignment.ReadAll(a => a!.VolunteerId == id).Any())
                throw new BO.BlDeletionException($"Cannot delete volunteer with ID={id} as they are handling calls.");
            _dal.Volunteer.Delete(id);
            VolunteerManager.Observers.NotifyListUpdated(); //stage 5                                                    
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

    /// <summary>
    /// Allows a volunteer to log in by verifying their username and password.
    /// </summary>
    /// <param name="name">The name of the volunteer.</param>
    /// <param name="pass">The password of the volunteer.</param>
    /// <returns>The role of the volunteer if login is successful.</returns>
    public BO.Enums.Role EnterSystem(string id, string pass)
    {
        try
        {
            var volunteer = _dal.Volunteer.ReadAll()
                .FirstOrDefault(v => v!.ID.ToString() == id && VolunteerManager.VerifyPassword(pass, v.Password!));
            if (volunteer is null)
                throw new BO.BlUnauthorizedException("Invalid ID or password");
            return (BO.Enums.Role)volunteer.Role;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Error accessing volunteers.", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Retrieves a list of volunteers, optionally filtered by call type and sorted by a specific field.
    /// </summary>
    /// <param name="callTypeFilter">Optional filter for the volunteer's call type.</param>
    /// <param name="fieldSort">Optional sorting field for the volunteers list.</param>
    /// <returns>A list of BO.VolunteerInList objects representing the volunteers.</returns>
    public IEnumerable<VolunteerInList> GetVolunteersFilterList(BO.Enums.CallType? callType)//stage 5
    {
        try
        {
            IEnumerable<VolunteerInList> volunteers;
            if (callType is null)
                volunteers = GetVolunteersList();
            else
                volunteers = GetVolunteersList().Where(v => v.CallType == callType);
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