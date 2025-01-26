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
                    CallId = call!.ID,
                    Address = call.Address,
                    CallDescription = call.CallDescription
                };
            }

            return new BO.Volunteer
            {
                Id = doVolunteer.ID,
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


//using BlApi;
//using Helpers;
//namespace BlImplementation;

//internal class VolunteerImplementation : IVolunteer
//{
//    private readonly DalApi.IDal Volunteer_dal = DalApi.Factory.Get;

//    public BO.Role Login(string username, string password)
//    {
//        try
//        {
//            var user = Volunteer_dal.Volunteer.ReadAll()
//                .FirstOrDefault(u => u.Name == username)
//                ?? throw new BO.InvalidCredentialsException("שם המשתמש או הסיסמה אינם נכונים.");

//            if (user.Password != password)
//                throw new BO.InvalidCredentialsException("שם המשתמש או הסיסמה אינם נכונים.");

//            return (BO.Role)user.Role;
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.DataAccessException("שגיאה בגישה לנתוני משתמשים.", ex);
//        }
//    }

//    public void Create(BO.Volunteer boVolunteer)
//    {
//        try
//        {
//            ValidateVolunteer(boVolunteer);

//            var existingUser = Volunteer_dal.Volunteer.ReadAll()
//                .FirstOrDefault(u => u.Email == boVolunteer.Email);

//            if (existingUser != null)
//                throw new BO.ValidationException("משתמש עם אימייל זה כבר קיים במערכת.");

//            var doVolunteer = new DO.Volunteer
//            {
//                Name = boVolunteer.Name,
//                Email = boVolunteer.Email,
//                Phone = boVolunteer.Phone,
//                Password = boVolunteer.Password,
//                Address = boVolunteer.Address,
//                Latitude = boVolunteer.Latitude,
//                Longitude = boVolunteer.Longitude,
//                Role = (DO.Role)boVolunteer.Role,
//                IsActive = true,
//                MaxDistanceForCall = boVolunteer.MaxDistanceForCall,
//                DistanceType = (DO.DistanceType)boVolunteer.DistanceType
//            };

//            Volunteer_dal.Volunteer.Create(doVolunteer);
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.DataAccessException("שגיאה ביצירת מתנדב חדש.", ex);
//        }
//    }

//    public BO.Volunteer? Read(int id)
//    {
//        try
//        {
//            var doVolunteer = Volunteer_dal.Volunteer.Read(id)
//                ?? throw new BO.NotFoundException("המתנדב לא נמצא.");

//            var assignments = Volunteer_dal.Assignment.ReadAll()
//                .Where(a => a.VolunteerId == id)
//                .ToList();

//            var currentAssignment = assignments.FirstOrDefault(a => a.EndTimeForTreatment == null);
//            BO.CallInProgress? callInProgress = null;

//            if (currentAssignment != null)
//            {
//                var call = Volunteer_dal.Call.Read(currentAssignment.CallId);
//                callInProgress = new BO.CallInProgress
//                {
//                    CallId = call.ID,
//                    Address = call.Address,
//                    CallDescription = call.CallDescription
//                };
//            }

//            return new BO.Volunteer
//            {
//                Id = doVolunteer.Id,
//                Name = doVolunteer.Name,
//                Email = doVolunteer.Email,
//                Phone = doVolunteer.Phone,
//                Password = doVolunteer.Password,
//                Address = doVolunteer.Address,
//                Latitude = doVolunteer.Latitude,
//                Longitude = doVolunteer.Longitude,
//                Role = (BO.Role)doVolunteer.Role,
//                IsActive = doVolunteer.IsActive,
//                MaxDistanceForCall = doVolunteer.MaxDistanceForCall,
//                DistanceType = (BO.DistanceType)doVolunteer.DistanceType,
//                TreatedCallNum = assignments.Count(a => a.TypeOfTreatmentEnding == DO.TypeOfFinishTreatment.Treated),
//                CenteledCallNum = assignments.Count(a => a.TypeOfTreatmentEnding == DO.TypeOfFinishTreatment.Canceled),
//                OutOfRangeCallNum = assignments.Count(a => a.TypeOfTreatmentEnding == DO.TypeOfFinishTreatment.OutOfRangeCancellation),
//                CallInProgress = callInProgress
//            };
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.DataAccessException("שגיאה בגישה לנתוני מתנדבים.", ex);
//        }
//    }

//    public IEnumerable<BO.VolunteerInList> ReadAll(BO.Active? sort = null, BO.VolunteerFields? filter = null, object? value = null)
//    {
//        try
//        {
//            var volunteers = Volunteer_dal.Volunteer.ReadAll();
//            var assignments = Volunteer_dal.Assignment.ReadAll();

//            // סינון לפי סטטוס פעיל/לא פעיל
//            if (sort.HasValue)
//                volunteers = volunteers.Where(v => v.IsActive == (sort == BO.Active.TRUE)).ToList();

//            var volunteerList = volunteers.Select(v =>
//            {
//                var volunteerAssignments = assignments.Where(a => a.VolunteerId == v.ID);
//                var currentAssignment = volunteerAssignments.FirstOrDefault(a => a.EndTimeForTreatment == null);

//                return new BO.VolunteerInList
//                {
//                    Id = v.ID,
//                    Name = v.Name,
//                    IsActive = v.IsActive,
//                    TreatedCallNum = volunteerAssignments.Count(a => a.TypeOfTreatmentEnding == DO.TypeOfFinishTreatment.Treated),
//                    CenteledCallNum = volunteerAssignments.Count(a => a.TypeOfTreatmentEnding == DO.TypeOfFinishTreatment.Canceled),
//                    OutOfRangeCallNum = volunteerAssignments.Count(a => a.TypeOfTreatmentEnding == DO.TypeOfFinishTreatment.OutOfRangeCancellation),
//                    CallId = currentAssignment?.CallId ?? 0,
//                    TypeOfCall = currentAssignment != null ?
//                        (BO.TypeOfCall)Volunteer_dal.Call.Read(currentAssignment.CallId).TypeOfCall :
//                        default
//                };
//            });

//            // מיון לפי שדה ספציפי
//            if (filter.HasValue)
//            {
//                volunteerList = filter switch
//                {
//                    BO.VolunteerFields.Name => volunteerList.OrderBy(v => v.Name),
//                    BO.VolunteerFields.Id => volunteerList.OrderBy(v => v.Id),
//                    _ => volunteerList
//                };
//            }

//            return volunteerList.ToList();
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.DataAccessException("שגיאה בגישה לנתוני מתנדבים.", ex);
//        }
//    }

//    public void Update(BO.Volunteer boVolunteer)
//    {
//        try
//        {
//            ValidateVolunteer(boVolunteer);

//            var existingVolunteer = Volunteer_dal.Volunteer.Read(boVolunteer.Id)
//                ?? throw new BO.NotFoundException("המתנדב לא נמצא.");

//            var doVolunteer = new DO.Volunteer
//            {
//                ID = boVolunteer.Id,
//                Name = boVolunteer.Name,
//                Email = boVolunteer.Email,
//                Phone = boVolunteer.Phone,
//                Password = boVolunteer.Password ?? existingVolunteer.Password,
//                Address = boVolunteer.Address,
//                Latitude = boVolunteer.Latitude,
//                Longitude = boVolunteer.Longitude,
//                Role = (DO.Role)boVolunteer.Role,
//                IsActive = boVolunteer.IsActive,
//                MaxDistanceForCall = boVolunteer.MaxDistanceForCall,
//                DistanceType = (DO.DistanceType)boVolunteer.DistanceType
//            };

//            Volunteer_dal.Volunteer.Update(doVolunteer);
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.DataAccessException("שגיאה בעדכון נתוני מתנדבים.", ex);
//        }
//    }

//    public void Delete(int id)
//    {
//        try
//        {
//            var activeAssignments = Volunteer_dal.Assignment.ReadAll()
//                .Where(a => a.VolunteerId == id && a.EndTimeForTreatment == null)
//                .ToList();

//            if (activeAssignments.Any())
//                throw new BO.InvalidOperationException("לא ניתן למחוק מתנדב שמטפל כרגע בקריאה.");

//            Volunteer_dal.Volunteer.Delete(id);
//        }
//        catch (DO.NotFoundException)
//        {
//            throw new BO.NotFoundException("המתנדב לא נמצא.");
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.DataAccessException("שגיאה במחיקת נתוני מתנדבים.", ex);
//        }
//    }

//    public void MatchVolunteerToCall(int volunteerId, int callId)
//    {
//        try
//        {
//            // בדיקה שהמתנדב קיים
//            var volunteer = Volunteer_dal.Volunteer.Read(volunteerId)
//                ?? throw new BO.NotFoundException("המתנדב לא נמצא.");

//            // בדיקה שהקריאה קיימת
//            var call = Volunteer_dal.Call.Read(callId)
//                ?? throw new BO.NotFoundException("הקריאה לא נמצאה.");

//            var existingAssignment = Volunteer_dal.Assignment.ReadAll()
//                .FirstOrDefault(a => a.VolunteerId == volunteerId && a.EndTimeForTreatment == null);

//            if (existingAssignment != null)
//                throw new BO.InvalidOperationException("לא ניתן להתאים מתנדב שכבר מטפל בקריאה אחרת.");

//            var newAssignment = new DO.Assignment
//            {
//                VolunteerId = volunteerId,
//                CallId = callId,
//                EntryTimeForTreatment = ClockManager.Now
//            };

//            Volunteer_dal.Assignment.Create(newAssignment);
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.DataAccessException("שגיאה בהתאמת מתנדב לקריאה.", ex);
//        }
//    }

//    public void UnMatchVolunteerToCall(int volunteerId, int callId)
//    {
//        try
//        {
//            var assignment = Volunteer_dal.Assignment.ReadAll()
//                .FirstOrDefault(a => a.VolunteerId == volunteerId && a.CallId == callId && a.EndTimeForTreatment == null)
//                ?? throw new BO.NotFoundException("לא נמצאה התאמה בין המתנדב לקריאה.");

//            assignment.EndTimeForTreatment = ClockManager.Now;
//            assignment.TypeOfTreatmentEnding = DO.TypeOfFinishTreatment.OutOfRangeCancellation;

//            Volunteer_dal.Assignment.Update(assignment);
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.DataAccessException("שגיאה בביטול התאמת מתנדב לקריאה.", ex);
//        }
//    }

//    private void ValidateVolunteer(BO.Volunteer volunteer)
//    {
//        if (string.IsNullOrWhiteSpace(volunteer.Name))
//            throw new BO.ValidationException("שם המתנדב אינו תקין.");
//        if (!IsValidEmail(volunteer.Email))
//            throw new BO.ValidationException("כתובת האימייל אינה תקינה.");
//        if (!IsValidPhone(volunteer.Phone))
//            throw new BO.ValidationException("מספר הטלפון אינו תקין.");
//        if (volunteer.MaxDistanceForCall <= 0)
//            throw new BO.ValidationException("מרחק מקסימלי לקריאה חייב להיות חיובי.");
//    }

//    private bool IsValidEmail(string email) =>
//        new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email);

//    private bool IsValidPhone(string phone) =>
//        phone.All(char.IsDigit) && phone.Length == 10;
//}

//using BlApi;
//using Helpers;
//namespace BlImplementation;

//internal class VolunteerImplementation : IVolunteer
//{
//    private readonly DalApi.IDal Volunteer_dal = DalApi.Factory.Get;

//    public BO.Role Login(string username, string password)
//    {
//        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
//            throw new BO.BlArgumentException("שם משתמש וסיסמה הם שדות חובה.");

//        try
//        {
//            var user = Volunteer_dal.Volunteer.ReadAll()
//                .FirstOrDefault(u => u.Name == username);

//            if (user == null || user.Password != password)
//                throw new BO.BlDoesNotExistException("שם המשתמש או הסיסמה אינם נכונים.");

//            return (BO.Role)user.Role;
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.BlDatabaseException("שגיאה בגישה לנתוני משתמשים.", ex);
//        }
//        catch (Exception ex)
//        {
//            throw new BO.BlGeneralException("שגיאה כללית בתהליך ההתחברות.", ex);
//        }
//    }

//    public void Create(BO.Volunteer boVolunteer)
//    {
//        if (boVolunteer == null)
//            throw new BO.BlNullReferenceException("פרטי המתנדב לא סופקו.");

//        try
//        {
//            ValidateVolunteer(boVolunteer);

//            var existingUser = Volunteer_dal.Volunteer.ReadAll()
//                .FirstOrDefault(u => u.Email == boVolunteer.Email);

//            if (existingUser != null)
//                throw new BO.BlAlreadyExistsException("משתמש עם אימייל זה כבר קיים במערכת.");

//            var doVolunteer = new DO.Volunteer
//            {
//                Name = boVolunteer.Name,
//                Email = boVolunteer.Email,
//                Phone = boVolunteer.Phone,
//                Password = boVolunteer.Password ?? throw new BO.BlNullPropertyException("סיסמה היא שדה חובה."),
//                Address = boVolunteer.Address,
//                Latitude = boVolunteer.Latitude,
//                Longitude = boVolunteer.Longitude,
//                Role = (DO.Role)boVolunteer.Role,
//                IsActive = true,
//                MaxDistanceForCall = boVolunteer.MaxDistanceForCall,
//                DistanceType = (DO.DistanceType)boVolunteer.DistanceType
//            };

//            Volunteer_dal.Volunteer.Create(doVolunteer);
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.BlDatabaseException("שגיאה ביצירת מתנדב חדש.", ex);
//        }
//        catch (BO.BlInvalidInputException)
//        {
//            throw;
//        }
//        catch (Exception ex)
//        {
//            throw new BO.BlGeneralException("שגיאה כללית ביצירת מתנדב.", ex);
//        }
//    }

//    public BO.Volunteer? Read(int id)
//    {
//        if (id <= 0)
//            throw new BO.BlArgumentException("מזהה מתנדב חייב להיות מספר חיובי.");

//        try
//        {
//            var doVolunteer = Volunteer_dal.Volunteer.Read(id)
//                ?? throw new BO.BlDoesNotExistException("המתנדב לא נמצא.");

//            var assignments = Volunteer_dal.Assignment.ReadAll()
//                .Where(a => a.VolunteerId == id)
//                .ToList();

//            var currentAssignment = assignments.FirstOrDefault(a => a.EndTimeForTreatment == null);
//            BO.CallInProgress? callInProgress = null;

//            if (currentAssignment != null)
//            {
//                var call = Volunteer_dal.Call.Read(currentAssignment.CallId)
//                    ?? throw new BO.BlSystemException("נמצאה התאמה לקריאה לא קיימת.");

//                callInProgress = new BO.CallInProgress
//                {
//                    CallId = call.ID,
//                    Address = call.Address,
//                    CallDescription = call.CallDescription
//                };
//            }

//            return new BO.Volunteer
//            {
//                Id = doVolunteer.ID,
//                Name = doVolunteer.Name,
//                Email = doVolunteer.Email,
//                Phone = doVolunteer.Phone,
//                Password = doVolunteer.Password,
//                Address = doVolunteer.Address,
//                Latitude = doVolunteer.Latitude,
//                Longitude = doVolunteer.Longitude,
//                Role = (BO.Role)doVolunteer.Role,
//                IsActive = doVolunteer.IsActive,
//                MaxDistanceForCall = doVolunteer.MaxDistanceForCall,
//                DistanceType = (BO.DistanceType)doVolunteer.DistanceType,
//                TreatedCallNum = assignments.Count(a => a.TypeOfTreatmentEnding == DO.TypeOfFinishTreatment.Treated),
//                CenteledCallNum = assignments.Count(a => a.TypeOfTreatmentEnding == DO.TypeOfFinishTreatment.Canceled),
//                OutOfRangeCallNum = assignments.Count(a => a.TypeOfTreatmentEnding == DO.TypeOfFinishTreatment.OutOfRangeCancellation),
//                CallInProgress = callInProgress
//            };
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.BlDatabaseException("שגיאה בגישה לנתוני מתנדבים.", ex);
//        }
//        catch (BO.BlDoesNotExistException)
//        {
//            throw;
//        }
//        catch (Exception ex)
//        {
//            throw new BO.BlGeneralException("שגיאה כללית בקריאת נתוני מתנדב.", ex);
//        }
//    }

//    public IEnumerable<BO.VolunteerInList> ReadAll(BO.Active? sort = null, BO.VolunteerFields? filter = null, object? value = null)
//    {
//        try
//        {
//            var volunteers = Volunteer_dal.Volunteer.ReadAll();
//            var assignments = Volunteer_dal.Assignment.ReadAll();

//            if (sort.HasValue)
//                volunteers = volunteers.Where(v => v.IsActive == (sort == BO.Active.TRUE)).ToList();

//            var volunteerList = volunteers.Select(v =>
//            {
//                var volunteerAssignments = assignments.Where(a => a.VolunteerId == v.ID);
//                var currentAssignment = volunteerAssignments.FirstOrDefault(a => a.EndTimeForTreatment == null);

//                DO.Call? currentCall = null;
//                if (currentAssignment != null)
//                {
//                    try
//                    {
//                        currentCall = Volunteer_dal.Call.Read(currentAssignment.CallId);
//                    }
//                    catch
//                    {
//                        throw new BO.BlSystemException("נמצאה התאמה לקריאה לא קיימת.");
//                    }
//                }

//                return new BO.VolunteerInList
//                {
//                    Id = v.ID,
//                    Name = v.Name,
//                    IsActive = v.IsActive,
//                    TreatedCallNum = volunteerAssignments.Count(a => a.TypeOfTreatmentEnding == DO.TypeOfFinishTreatment.Treated),
//                    CenteledCallNum = volunteerAssignments.Count(a => a.TypeOfTreatmentEnding == DO.TypeOfFinishTreatment.Canceled),
//                    OutOfRangeCallNum = volunteerAssignments.Count(a => a.TypeOfTreatmentEnding == DO.TypeOfFinishTreatment.OutOfRangeCancellation),
//                    CallId = currentCall?.ID ?? 0,
//                    TypeOfCall = currentCall != null ? (BO.TypeOfCall)currentCall.TypeOfCall : default
//                };
//            });

//            if (filter.HasValue)
//            {
//                volunteerList = filter switch
//                {
//                    BO.VolunteerFields.Name => volunteerList.OrderBy(v => v.Name),
//                    BO.VolunteerFields.Id => volunteerList.OrderBy(v => v.Id),
//                    _ => volunteerList
//                };
//            }

//            return volunteerList.ToList();
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.BlDatabaseException("שגיאה בגישה לנתוני מתנדבים.", ex);
//        }
//        catch (Exception ex)
//        {
//            throw new BO.BlGeneralException("שגיאה כללית בקריאת רשימת מתנדבים.", ex);
//        }
//    }

//    public void Update(BO.Volunteer boVolunteer)
//    {
//        if (boVolunteer == null)
//            throw new BO.BlNullReferenceException("פרטי המתנדב לא סופקו.");

//        if (boVolunteer.Id <= 0)
//            throw new BO.BlArgumentException("מזהה מתנדב חייב להיות מספר חיובי.");

//        try
//        {
//            ValidateVolunteer(boVolunteer);

//            var existingVolunteer = Volunteer_dal.Volunteer.Read(boVolunteer.Id)
//                ?? throw new BO.BlDoesNotExistException("המתנדב לא נמצא.");

//            var doVolunteer = new DO.Volunteer
//            {
//                ID = boVolunteer.Id,
//                Name = boVolunteer.Name,
//                Email = boVolunteer.Email,
//                Phone = boVolunteer.Phone,
//                Password = boVolunteer.Password ?? existingVolunteer.Password,
//                Address = boVolunteer.Address,
//                Latitude = boVolunteer.Latitude,
//                Longitude = boVolunteer.Longitude,
//                Role = (DO.Role)boVolunteer.Role,
//                IsActive = boVolunteer.IsActive,
//                MaxDistanceForCall = boVolunteer.MaxDistanceForCall,
//                DistanceType = (DO.DistanceType)boVolunteer.DistanceType
//            };

//            Volunteer_dal.Volunteer.Update(doVolunteer);
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.BlDatabaseException("שגיאה בעדכון נתוני מתנדבים.", ex);
//        }
//        catch (BO.BlInvalidInputException)
//        {
//            throw;
//        }
//        catch (BO.BlDoesNotExistException)
//        {
//            throw;
//        }
//        catch (Exception ex)
//        {
//            throw new BO.BlGeneralException("שגיאה כללית בעדכון נתוני מתנדב.", ex);
//        }
//    }

//    public void Delete(int id)
//    {
//        if (id <= 0)
//            throw new BO.BlArgumentException("מזהה מתנדב חייב להיות מספר חיובי.");

//        try
//        {
//            var activeAssignments = Volunteer_dal.Assignment.ReadAll()
//                .Where(a => a.VolunteerId == id && a.EndTimeForTreatment == null)
//                .ToList();

//            if (activeAssignments.Any())
//                throw new BO.BlInvalidInputException("לא ניתן למחוק מתנדב שמטפל כרגע בקריאה.");

//            if (!Volunteer_dal.Volunteer.Read(id).HasValue)
//                throw new BO.BlDoesNotExistException("המתנדב לא נמצא.");

//            Volunteer_dal.Volunteer.Delete(id);
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.BlDatabaseException("שגיאה במחיקת נתוני מתנדבים.", ex);
//        }
//        catch (BO.BlInvalidInputException)
//        {
//            throw;
//        }
//        catch (BO.BlDoesNotExistException)
//        {
//            throw;
//        }
//        catch (Exception ex)
//        {
//            throw new BO.BlGeneralException("שגיאה כללית במחיקת מתנדב.", ex);
//        }
//    }

//    public void MatchVolunteerToCall(int volunteerId, int callId)
//    {
//        if (volunteerId <= 0 || callId <= 0)
//            throw new BO.BlArgumentException("מזהה מתנדב וקריאה חייבים להיות מספרים חיוביים.");

//        try
//        {
//            var volunteer = Volunteer_dal.Volunteer.Read(volunteerId)
//                ?? throw new BO.BlDoesNotExistException("המתנדב לא נמצא.");

//            var call = Volunteer_dal.Call.Read(callId)
//                ?? throw new BO.BlDoesNotExistException("הקריאה לא נמצאה.");

//            var existingAssignment = Volunteer_dal.Assignment.ReadAll()
//                .FirstOrDefault(a => a.VolunteerId == volunteerId && a.EndTimeForTreatment == null);

//            if (existingAssignment != null)
//                throw new BO.BlInvalidInputException("לא ניתן להתאים מתנדב שכבר מטפל בקריאה אחרת.");

//            var newAssignment = new DO.Assignment
//            {
//                VolunteerId = volunteerId,
//                CallId = callId,
//                EntryTimeForTreatment = ClockManager.Now
//            };

//            Volunteer_dal.Assignment.Create(newAssignment);
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.BlDatabaseException("שגיאה בהתאמת מתנדב לקריאה.", ex);
//        }
//        catch (BO.BlDoesNotExistException)
//        {
//            throw;
//        }
//        catch (BO.BlInvalidInputException)
//        {
//            throw;
//        }
//        catch (Exception ex)
//        {
//            throw new BO.BlGeneralException("שגיאה כללית בהתאמת מתנדב לקריאה.", ex);
//        }
//    }

//    public void UnMatchVolunteerToCall(int volunteerId, int callId)
//    {
//        if (volunteerId <= 0 || callId <= 0)
//            throw new BO.BlArgumentException("מזהה מתנדב וקריאה חייבים להיות מספרים חיוביים.");

//        try
//        {
//            var assignment = Volunteer_dal.Assignment.ReadAll()
//                .FirstOrDefault(a => a.VolunteerId == volunteerId && a.CallId == callId && a.EndTimeForTreatment == null)
//                ?? throw new BO.BlDoesNotExistException("לא נמצאה התאמה בין המתנדב לקריאה.");

//            assignment.EndTimeForTreatment = ClockManager.Now;
//            assignment.TypeOfTreatmentEnding = DO.TypeOfFinishTreatment.OutOfRangeCancellation;

//            Volunteer_dal.Assignment.Update(assignment);
//        }
//        catch (DO.DataAccessException ex)
//        {
//            throw new BO.DataAccessException("שגיאה בביטול התאמת מתנדב לקריאה.", ex);
//        }
//    }

//    private void ValidateVolunteer(BO.Volunteer volunteer)
//    {
//        if (string.IsNullOrWhiteSpace(volunteer.Name))
//            throw new BO.ValidationException("שם המתנדב אינו תקין.");
//        if (!IsValidEmail(volunteer.Email))
//            throw new BO.ValidationException("כתובת האימייל אינה תקינה.");
//        if (!IsValidPhone(volunteer.Phone))
//            throw new BO.ValidationException("מספר הטלפון אינו תקין.");
//        if (volunteer.MaxDistanceForCall <= 0)
//            throw new BO.ValidationException("מרחק מקסימלי לקריאה חייב להיות חיובי.");
//    }
    
//    private bool IsValidEmail(string email) =>
//        new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email);

//    private bool IsValidPhone(string phone) =>
//        phone.All(char.IsDigit) && phone.Length == 10;
//}