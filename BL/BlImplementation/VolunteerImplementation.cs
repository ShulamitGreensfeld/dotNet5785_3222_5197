using BlApi;
using Helpers;

namespace BlImplementation;

internal class VolunteerImplementation : IVolunteer
{

    private readonly DalApi.IDal _dal = DalApi.Factory.Get;


    public BO.Volunteer GetVolunteerDetails(int id)
    {
        try
        {
            var doVolunteer = _dal.Volunteer.Read(id) ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={id} does Not exist");
            return VolunteerManager.ConvertDoVolunteerToBoVolunteer(doVolunteer);
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
                BO.Enums.VolunteerInListFields.CallId => allVolunteersInList.OrderBy(v => v?.TotalExpiredCalls).ToList(),
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
            throw new BO.BlGeneralException("An unexpected error occurred.", ex);
        }
    }
    public void AddVolunteer(BO.Volunteer boVolunteer)
    {
        try
        {
            // בודק אם כבר קיים מתנדב עם אותו ID
            if (_dal.Volunteer.Read(boVolunteer.Id) is not null)
                throw new BO.BlDoesNotExistException($"Volunteer with ID={boVolunteer.Id} already exists");

            // מאמת את המתנדב לפני יצירתו
            VolunteerManager.ValidateVolunteer(boVolunteer);

            // אם יש סיסמה חדשה, מצפין אותה
            if (!string.IsNullOrEmpty(boVolunteer.Password))
            {
                boVolunteer.Password = VolunteerManager.EncryptPassword(boVolunteer.Password); // הצפנה של הסיסמה
            }

            // אם יש כתובת, ניתן להוסיף כאן קוד לקבלת קואורדינטות (אם תרצה להפעיל את זה שוב)
            var (latitude, longitude) = Tools.GetCoordinatesFromAddress(boVolunteer.FullAddress!);
            if (latitude == null || longitude == null)
                throw new BO.BlInvalidFormatException($"Invalid address: {boVolunteer.FullAddress}");
            boVolunteer.Latitude = latitude;
            boVolunteer.Longitude = longitude;

            // ממיר את המתנדב מ-BO ל-DO
            DO.Volunteer doVolunteer = VolunteerManager.ConvertBoVolunteerToDoVolunteer(boVolunteer);

            // שומר את המתנדב בבסיס הנתונים
            _dal.Volunteer.Create(doVolunteer);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistException($"Volunteer with ID={boVolunteer.Id} already exists", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException("An unexpected error occurred.", ex);
        }
    }


    public void UpdateVolunteerDetails(int requesterId, BO.Volunteer boVolunteer)
    {
        try
        {
            DO.Volunteer? requester = _dal.Volunteer.Read(requesterId) ?? throw new BO.BlDoesNotExistException("Requester does not exist!");
            if (requester.ID != boVolunteer.Id && requester.Role != DO.Role.Manager)
                throw new BO.BlUnauthorizedException("Requester is not authorized!");
            var existingVolunteer = _dal.Volunteer.Read(boVolunteer.Id) ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={boVolunteer.Id} does not exist");
            VolunteerManager.ValidateVolunteer(boVolunteer);

            if (string.IsNullOrEmpty(boVolunteer.Password!) && !VolunteerManager.IsPasswordStrong(boVolunteer.Password!))
                throw new BO.BlInvalidFormatException("Password is not strong!");
            if (requester.Role != DO.Role.Manager && requester.Role != (DO.Role)boVolunteer.Role)
                throw new BO.BlUnauthorizedException("Requester is not authorized to change the Role field!");
            var (latitude, longitude) = Tools.GetCoordinatesFromAddress(boVolunteer.FullAddress!);
            if (latitude == null || longitude == null)
                throw new BO.BlInvalidFormatException($"Invalid address: {boVolunteer.FullAddress}");
            boVolunteer.Latitude = latitude;
            boVolunteer.Longitude = longitude;

            DO.Volunteer updatedVolunteer = VolunteerManager.ConvertBoVolunteerToDoVolunteer(boVolunteer);
            _dal.Volunteer.Update(updatedVolunteer);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Volunteer with ID={boVolunteer.Id} does not exist", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException("An unexpected error occurred.", ex);
        }
    }
    public void DeleteVolunteer(int id)
    {
        try
        {
            var volunteer = _dal.Volunteer.Read(id) ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={id} does not exist");
            if (_dal.Assignment.ReadAll(a => a!.VolunteerId == id).Any())
                throw new BO.BlDeletionException($"Cannot delete volunteer with ID={id} as he is handling calls.");
            _dal.Volunteer.Delete(id);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Volunteer with ID={id} does not exist", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException("An unexpected error occurred.", ex);
        }
    }
    public BO.Enums.Role EnterSystem(string name, string pass)
    {
        try
        {
            var volunteer = _dal.Volunteer.ReadAll().FirstOrDefault(v => v!.Name == name && VolunteerManager.VerifyPassword(pass, v.Password!));
            return volunteer is null ? throw new BO.BlUnauthorizedException("Invalid username or password") : (BO.Enums.Role)volunteer.Role;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Error accessing volunteers.", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlGeneralException("An unexpected error occurred.", ex);
        }
    }
}