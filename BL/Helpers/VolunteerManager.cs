using DalApi;
using DO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static BO.Enums;

namespace Helpers;

internal static class VolunteerManager
{
    private readonly static IDal s_dal = Factory.Get;
    public static BO.Volunteer ConvertDoVolunteerToBoVolunteer(DO.Volunteer doVolunteer)
    {
        try
        {
            var currentVolunteerAssignments = s_dal.Assignment.ReadAll(a => a?.VolunteerId == doVolunteer.ID);
            var totalHandled = currentVolunteerAssignments.Count(a => a?.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.Treated);
            var totalCanceled = currentVolunteerAssignments.Count(a => a?.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.ManagerCancellation || a!.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.SelfCancellation);
            var totalExpired = currentVolunteerAssignments.Count(a => a?.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.Treated);
            var assignedCallId = currentVolunteerAssignments.FirstOrDefault(a => a?.EndTimeForTreatment == null)?.CallId;
            var currentAssignment = s_dal.Assignment.ReadAll(a => a.VolunteerId == doVolunteer.ID && a.EndTimeForTreatment == null).FirstOrDefault();
            BO.CallInProgress? callInProgress = null;
            if (currentAssignment is not null)
            {
                var callDetails = s_dal.Call.Read(currentAssignment.CallId);
                if (callDetails is not null)
                {
                    callInProgress = new BO.CallInProgress
                    {
                        Id = currentAssignment.ID,
                        CallId = currentAssignment.CallId,
                        CallType = (BO.Enums.CallType)callDetails.TypeOfCall,
                        Verbal_description = callDetails.CallDescription,
                        FullAddress = callDetails.Address,
                        Opening_time = callDetails.OpeningTime,
                        Max_finish_time = (DateTime)callDetails.MaxTimeForClosing!,
                        Start_time = currentAssignment.EntryTimeForTreatment,
                        CallDistance = Tools.CalculateDistance(doVolunteer.Latitude, doVolunteer.Longitude, callDetails.Latitude, callDetails.Longitude),
                        CallStatus = CallManager.CalculateCallStatus(callDetails)
                    };
                }
            }
            return new BO.Volunteer
            {
                Id = doVolunteer.ID,
                FullName = doVolunteer.Name,
                CellphoneNumber = doVolunteer.Phone,
                Email = doVolunteer.Email,
                Password = doVolunteer.Password,
                FullAddress = doVolunteer.Address,
                Latitude = doVolunteer?.Latitude,
                Longitude = doVolunteer?.Longitude,
                Role = (BO.Enums.Role)doVolunteer!.Role,
                IsActive = doVolunteer.IsActive,
                DistanceType = (BO.Enums.DistanceTypes)doVolunteer.DistanceType,
                MaxDistance = doVolunteer.MaxDistanceForCall,
                TotalHandledCalls = totalHandled,
                TotalCanceledCalls = totalCanceled,
                TotalExpiredCalls = totalExpired,


            };
        }
        catch (DO.DalDoesNotExistException ex)
        {
            return null!;
        }
    }

    public static DO.Volunteer ConvertBoVolunteerToDoVolunteer(BO.Volunteer boVolunteer)
    {
        return new DO.Volunteer(
    boVolunteer.Id,
    boVolunteer.FullName,
    boVolunteer.CellphoneNumber,
    boVolunteer.Email,
    (DO.Role)boVolunteer.Role,
    boVolunteer.IsActive,
    (DO.DistanceType)boVolunteer.DistanceType,
    boVolunteer.MaxDistance,
    boVolunteer.Password,
    boVolunteer.FullAddress,
    boVolunteer.Latitude,
    boVolunteer.Longitude
    );
    }

    public static BO.VolunteerInList ConvertDoVolunteerToBoVolunteerInList(DO.Volunteer doVolunteer)
    {
        var currentVolunteerAssignments = s_dal.Assignment.ReadAll(a => a?.VolunteerId == doVolunteer.ID);

        var totalHandled = currentVolunteerAssignments.Count(a => a?.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.Treated);
        var totalCanceled = currentVolunteerAssignments.Count(a => a?.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.ManagerCancellation || a!.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.SelfCancellation);
        var totalExpired = currentVolunteerAssignments.Count(a => a?.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.Treated);
        var assignedCallId = currentVolunteerAssignments.FirstOrDefault(a => a?.EndTimeForTreatment == null)?.CallId;

        return new BO.VolunteerInList
        {
            Id = doVolunteer.ID,
            FullName = doVolunteer.Name,
            IsActive = doVolunteer.IsActive,
            TotalHandledCalls = totalHandled,
            TotalCanceledCalls = totalCanceled,
            TotalExpiredCalls = totalExpired,
            CallId = assignedCallId,
            CallType = assignedCallId is not null
                ? (BO.Enums.CallType)(s_dal.Call.Read(assignedCallId.Value)?.TypeOfCall ?? DO.TypeOfCall.ToPackageFood)
                : BO.Enums.CallType.ToPackageFood
        };
    }
    public static List<BO.VolunteerInList> GetVolunteerList(IEnumerable<DO.Volunteer> volunteers)
          => volunteers.Select(v => ConvertDoVolunteerToBoVolunteerInList(v)).ToList();

    internal static bool VerifyPassword(string enteredPassword, string storedPassword)
    {
        var encryptedEnteredPassword = EncryptPassword(enteredPassword);
        return encryptedEnteredPassword == storedPassword;
    }

    internal static string EncryptPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower(); // הצפנה ב-SHA256 כ-string
    }

    internal static string GenerateStrongPassword()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#$%^&*";
        return new string(Enumerable.Repeat(chars, 12) // אורך 12 תווים
                               .Select(s => s[random.Next(s.Length)])
                               .ToArray());
    }

    public static bool IsValidId(int id)
    {
        string idStr = id.ToString().PadLeft(9, '0');

        // בדיקה שהאורך הוא 9 ושיש לפחות ספרה אחת שאינה 0
        if (idStr.Length != 9 || idStr.All(c => c == '0')) 
            return false;

        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            int digit = idStr[i] - '0';
            int weight = (i % 2 == 0) ? 1 : 2;
            int product = digit * weight;

            sum += (product > 9) ? product - 9 : product;
        }
        return sum % 10 == 0;
    }
    public static void ValidateVolunteer(BO.Volunteer volunteer)
    {
        if (volunteer.Id <= 0 || !IsValidId(volunteer.Id))
            throw new BO.BlInvalidFormatException("Invalid Id number!");

        if (string.IsNullOrEmpty(volunteer.FullName))
            throw new BO.BlInvalidFormatException("Invalid name!");

        if (!Regex.IsMatch(volunteer.CellphoneNumber, @"^\d{3}-?\d{7}$"))
            throw new BO.BlInvalidFormatException("Invalid cellphone number!");

        if (!Regex.IsMatch(volunteer.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new BO.BlInvalidFormatException("Invalid email!");

        if (volunteer.Password == null && (volunteer.Password!.Length > 12 ||
             !Regex.IsMatch(volunteer.Password, "[A-Z]") ||
             !Regex.IsMatch(volunteer.Password, "[a-z]") ||
             !Regex.IsMatch(volunteer.Password, "[0-9]") ||
             !Regex.IsMatch(volunteer.Password, "[!@#$%^&=*]")))
               throw new BO.BlInvalidFormatException("Invalid password!");

        if (volunteer.Role != (BO.Enums.Role.volunteer) && volunteer.Role != (BO.Enums.Role.manager))  
            throw new BO.BlInvalidFormatException("Invalid role!");

        if (volunteer.MaxDistance < 0)
            throw new BO.BlInvalidFormatException("Invalid distance!");

        if (!Enum.IsDefined(typeof(BO.Enums.DistanceTypes), volunteer.DistanceType))
            throw new BO.BlInvalidFormatException("Invalid distance type!");

        if (volunteer.TotalHandledCalls < 0)
            throw new BO.BlInvalidFormatException("Invalid sum of handled calls!");

        if (volunteer.TotalCanceledCalls < 0)
            throw new BO.BlInvalidFormatException("Invalid sum of canceled calls!");

        if (volunteer.TotalExpiredCalls < 0)
            throw new BO.BlInvalidFormatException("Invalid sum of expired calls!");
    }
    internal static bool IsPasswordStrong(string password)
    {
        if (password.Length < 8)
            return false;
        if (!password.Any(char.IsUpper))
            return false;
        if (!password.Any(char.IsLower))
            return false;
        if (!password.Any(char.IsDigit))
            return false;
        if (!password.Any(c => "@#$%^&*".Contains(c)))
            return false;
        return true;
    }

    //internal static void PeriodicVolunteersUpdates(DateTime oldClock, DateTime newClock)
    //{
    //    throw new NotImplementedException();
    //}
    //internal static void PeriodicVolunteersUpdates(DateTime oldClock, DateTime newClock)
    //{
    //    var assignments = s_dal.Assignment.ReadAll().ToList(); // שליפת כל המטלות הפעילות

    //    foreach (var assignment in assignments)
    //    {
    //        var call = s_dal.Call.Read(assignment.CallId); // שליפת הקריאה מתוך מסד הנתונים

    //        if (call != null && assignment.EndTimeForTreatment == null) // אם הקריאה עדיין פתוחה
    //        {
    //            // חישוב הזמן שנותר לטיפול
    //            TimeSpan remainingTime = (call.MaxTimeForClosing ?? newClock) - newClock;

    //            // קביעת סטטוס הקריאה
    //            bool isAtRisk = remainingTime <= TimeSpan.FromHours(2); // האם נותרו פחות משעתיים?
    //            bool isExpired = remainingTime <= TimeSpan.Zero; // האם נגמר הזמן?

    //            // עדכון המידע של המשימה במסד הנתונים
    //            //assignment.call.RiskRange = isAtRisk;
    //            //assignment.EntryTimeForTreatment = isExpired ? TimeSpan.Zero : remainingTime; // אם נגמר הזמן - 0

    //            s_dal.Assignment.Update(assignment);
    //        }
    //    }
    //}

    //public static void PeriodicVolunteersUpdates(DateTime oldClock, DateTime newClock)
    //{
    //    // Get all active assignments (those without EndTimeForTreatment)
    //    var activeAssignments = s_dal.Assignment.ReadAll(a => a.EndTimeForTreatment == null);

    //    foreach (var assignment in activeAssignments)
    //    {
    //        // Get the related call for this assignment
    //        var call = s_dal.Call.Read(assignment.CallId);

    //        if (call == null)
    //            continue;

    //        // If MaxTimeForClosing is defined for the call
    //        if (call.MaxTimeForClosing.HasValue)
    //        {
    //            // Check if the assignment has entered risk range with the new clock
    //            if (call.RiskRange.HasValue &&
    //                newClock >= call.MaxTimeForClosing.Value.Subtract(call.RiskRange.Value) &&
    //                (oldClock < call.MaxTimeForClosing.Value.Subtract(call.RiskRange.Value) || oldClock > newClock))
    //            {
    //                // Assignment has entered risk range - update status or trigger notification
    //                // (Implementation depends on your existing notification system)
    //                // This might involve updating a field in the assignment or triggering an event
    //            }

    //            // Check if the assignment has expired with the new clock
    //            if (newClock >= call.MaxTimeForClosing.Value &&
    //                (oldClock < call.MaxTimeForClosing.Value || oldClock > newClock))
    //            {
    //                // Assignment has expired - mark as expired
    //                //assignment.EndTimeForTreatment = newClock;
    //                //assignment.TypeOfFinishTreatment = TypeOfFinishTreatment.Treated;
    //                s_dal.Assignment.Update(assignment);
    //            }
    //        }
    //    }
    //}

    public static void PeriodicVolunteersUpdates(DateTime oldClock, DateTime newClock)
    {
        Console.WriteLine("Starting PeriodicVolunteersUpdates...");

        // Get all active assignments (those without EndTimeForTreatment)
        var activeAssignments = s_dal.Assignment.ReadAll(a => a.EndTimeForTreatment == null);
        Console.WriteLine($"Found {activeAssignments.Count()} active assignments.");

        foreach (var assignment in activeAssignments)
        {
            // Get the related call for this assignment
            var call = s_dal.Call.Read(assignment.CallId);

            if (call == null)
            {
                Console.WriteLine($"Call with ID {assignment.CallId} not found.");
                continue;
            }

            // If MaxTimeForClosing is defined for the call
            if (call.MaxTimeForClosing.HasValue)
            {
                // Check if the assignment has entered risk range with the new clock
                if (call.RiskRange.HasValue &&
                    newClock >= call.MaxTimeForClosing.Value.Subtract(call.RiskRange.Value) &&
                    (oldClock < call.MaxTimeForClosing.Value.Subtract(call.RiskRange.Value) || oldClock > newClock))
                {
                    // Assignment has entered risk range - update status or trigger notification
                    Console.WriteLine($"Assignment {assignment.ID} has entered risk range.");
                    // (Implementation depends on your existing notification system)
                    // This might involve updating a field in the assignment or triggering an event
                }

                // Check if the assignment has expired with the new clock
                if (newClock >= call.MaxTimeForClosing.Value &&
                    (oldClock < call.MaxTimeForClosing.Value || oldClock > newClock))
                {
                    // Assignment has expired - mark as expired
                    Console.WriteLine($"Assignment {assignment.ID} has expired.");
                    //assignment.EndTimeForTreatment = newClock;
                    //assignment.TypeOfFinishTreatment = TypeOfFinishTreatment.Treated;
                    s_dal.Assignment.Update(assignment);
                }
            }
        }

        Console.WriteLine("Finished PeriodicVolunteersUpdates.");
    }
    public static class DistanceCalculator
    {
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2, DistanceTypes distanceType)
        {
            switch (distanceType)
            {
                case DistanceTypes.aerial_distance:
                    return CalculateAerialDistance(lat1, lon1, lat2, lon2);
                case DistanceTypes.walking_distance:
                    return CalculateWalkingDistance(lat1, lon1, lat2, lon2);
                case DistanceTypes.driving_distance:
                    return CalculateDrivingDistance(lat1, lon1, lat2, lon2);
                default:
                    throw new ArgumentException("Invalid distance type");
            }
        }

        private static double CalculateAerialDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Implement aerial distance calculation
            return 0.0;
        }

        private static double CalculateWalkingDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Implement walking distance calculation
            return 0.0;
        }

        private static double CalculateDrivingDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Implement driving distance calculation
            return 0.0;
        }
    }
}



