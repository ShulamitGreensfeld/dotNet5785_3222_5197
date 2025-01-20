using BlApi;
using Helpers;
namespace BlImplementation;

internal class VolunteerImplementation : IVolunteer
{
    private readonly DalApi.IDal Volunteer_dal = DalApi.Factory.Get;
    public BO.Role Login(string username, string password)
    {
        return 0;
    }

    public void Create(BO.Volunteer boVolunteer)
    {
        try
        {
            var user = Volunteer_dal.Volunteer.ReadAll()
                .FirstOrDefault(u => u.Name == username);

            if (user == null || user.Password != password)
                throw new BO.InvalidCredentialsException("שם המשתמש או הסיסמה אינם נכונים.");

            return (BO.Role)user.Role;
        }
        catch (DO.DataAccessException ex)
        {
            throw new BO.DataAccessException("שגיאה בגישה לנתוני משתמשים.", ex);
        }
    }

    private void ValidateVolunteer(BO.Volunteer volunteer)
    {
        if (string.IsNullOrWhiteSpace(volunteer.Name))
            throw new BO.ValidationException("שם המתנדב אינו תקין.");
        if (!IsValidEmail(volunteer.Email))
            throw new BO.ValidationException("כתובת האימייל אינה תקינה.");
        if (!IsValidPhone(volunteer.Phone))
            throw new BO.ValidationException("מספר הטלפון אינו תקין.");
    }

    private bool IsValidEmail(string email) =>
        new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email);

    private bool IsValidPhone(string phone) =>
        phone.All(char.IsDigit) && phone.Length == 10;

    public void Delete(int id)
    {
        try
        {
            var activeAssignments = Volunteer_dal.Assignment.ReadAll()
                .Where(a => a.VolunteerId == id && a.EndTimeForTreatment == null)
                .ToList();

            if (activeAssignments.Any())
                throw new BO.InvalidOperationException("לא ניתן למחוק מתנדב שמטפל כרגע בקריאה.");

            Volunteer_dal.Volunteer.Delete(id);
        }
        catch (DO.DataAccessException ex)
        {
            throw new BO.DataAccessException("שגיאה במחיקת נתוני מתנדבים.", ex);
        }
    }

    public void MatchVolunteerToCall(int volunteerId, int callId)
    {
        try
        {
            var existingAssignment = Volunteer_dal.Assignment.ReadAll()
                .FirstOrDefault(a => a.VolunteerId == volunteerId && a.EndTimeForTreatment == null);

            if (existingAssignment != null)
                throw new BO.InvalidOperationException("לא ניתן להתאים מתנדב שכבר מטפל בקריאה אחרת.");

            var newAssignment = new DO.Assignment
            {
                VolunteerId = volunteerId,
                CallId = callId,
                EntryTimeForTreatment = ClockManager.Now
            };

            Volunteer_dal.Assignment.Create(newAssignment);
        }
        catch (DO.DataAccessException ex)
        {
            throw new BO.DataAccessException("שגיאה בהתאמת מתנדב לקריאה.", ex);
        }
    }

    public BO.Volunteer? Read(int id)
    {
        try
        {
            var doVolunteer = Volunteer_dal.Volunteer.Read(id)
                ?? throw new BO.NotFoundException("המתנדב לא נמצא.");

            var assignment = Volunteer_dal.Assignment.ReadAll()
                .FirstOrDefault(a => a.VolunteerId == id && a.EndTimeForTreatment == null);

            BO.CallInProgress? callInProgress = null;

            if (assignment != null)
            {
                var call = Volunteer_dal.Call.Read(assignment.CallId);
                callInProgress = new BO.CallInProgress
                {
                    CallId = call.ID,
                    Address = call.Address,
                    CallDescription = call.CallDescription
                };
            }

            return new BO.Volunteer
            {
                Id = doVolunteer.Id,
                Name = doVolunteer.Name,
                Email = doVolunteer.Email,
                Phone = doVolunteer.Phone,
                CallInProgress = callInProgress
            };
        }
        catch (DO.DataAccessException ex)
        {
            throw new BO.DataAccessException("שגיאה בגישה לנתוני מתנדבים.", ex);
        }
    }

    public IEnumerable<BO.VolunteerInList> ReadAll(BO.Active? sort = null, BO.VolunteerFields? filter = null, object? value = null)
    {
        try
        {
            var volunteers = Volunteer_dal.Volunteer.ReadAll();

            // סינון לפי סטטוס
            if (sort.HasValue)
                volunteers = volunteers.Where(v => v.IsActive == (sort == BO.Active.TRUE)).ToList();

            var volunteerList = volunteers.Select(v => new BO.VolunteerInList
            {
                Id = v.ID,
                Name = v.Name,
                IsActive = v.IsActive
            });

            // מיון לפי שדה ספציפי
            if (filter.HasValue)
            {
                volunteerList = filter switch
                {
                    BO.VolunteerFields.Name => volunteerList.OrderBy(v => v.Name),
                    BO.VolunteerFields.Id => volunteerList.OrderBy(v => v.Id),
                    _ => volunteerList.OrderBy(v => v.Id)
                };
            }

            return volunteerList.ToList();
        }
        catch (DO.DataAccessException ex)
        {
            throw new BO.DataAccessException("שגיאה בגישה לנתוני מתנדבים.", ex);
        }
    }

    public void UnMatchVolunteerToCall(int volunteerId, int callId)
    {
        try
        {
            var assignment = Volunteer_dal.Assignment.ReadAll()
                .FirstOrDefault(a => a.VolunteerId == volunteerId && a.CallId == callId && a.EndTimeForTreatment == null);

            if (assignment == null)
                throw new BO.NotFoundException("לא נמצאה התאמה בין המתנדב לקריאה.");

            assignment.TreatmentEndTime = ClockManager.Now;
            assignment.TypeOfTreatmentEnding = DO.TypeOfFinishTreatment.OutOfRangeCancellation;

            Volunteer_dal.Assignment.Update(assignment);
        }
        catch (DO.DataAccessException ex)
        {
            throw new BO.DataAccessException("שגיאה בביטול התאמת מתנדב לקריאה.", ex);
        }
    }

    public void Update(BO.Volunteer boVolunteer)
    {
        try
        {
            ValidateVolunteer(boVolunteer);

            var doVolunteer = new DO.Volunteer
            {
                ID = boVolunteer.Id,
                Name = boVolunteer.Name,
                Email = boVolunteer.Email,
                Phone = boVolunteer.Phone,
                IsActive = boVolunteer.IsActive
            };

            Volunteer_dal.Volunteer.Update(doVolunteer);
        }
        catch (DO.DataAccessException ex)
        {
            throw new BO.DataAccessException("שגיאה בעדכון נתוני מתנדבים.", ex);
        }
    }

}