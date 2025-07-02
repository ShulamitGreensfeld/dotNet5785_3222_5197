using BlApi;
using BlImplementation;
using BO;
using DalApi;
using DO;
using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Helpers;

internal static class VolunteerManager
{
    private readonly static IDal s_dal = DalApi.Factory.Get;
    private readonly static IBl s_bl = BlApi.Factory.Get();
    internal static ObserverManager Observers = new(); //stage 5 
    public static event Action? CallsListUpdated;
    private static readonly ConcurrentDictionary<int, Action<DO.Call>> callObservers = new();

    public static async Task<BO.Volunteer> ConvertDoVolunteerToBoVolunteer(DO.Volunteer doVolunteer)
    {
        try
        {
            IEnumerable<DO.Assignment> currentVolunteerAssignments;
            DO.Assignment? currentAssignment;

            lock (AdminManager.BlMutex)
            {
                currentVolunteerAssignments = s_dal.Assignment.ReadAll(a => a?.VolunteerId == doVolunteer.ID);
                currentAssignment = s_dal.Assignment.ReadAll(a => a.VolunteerId == doVolunteer.ID && a.EndTimeForTreatment == null).FirstOrDefault();
            }

            var totalHandled = currentVolunteerAssignments.Count(a => a?.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.Treated);
            var totalCanceled = currentVolunteerAssignments.Count(a => a?.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.ManagerCancellation || a!.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.SelfCancellation);
            var totalExpired = currentVolunteerAssignments.Count(a => a?.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.OutOfRangeCancellation);

            BO.CallInProgress? callInProgress = null;
            BO.Enums.DistanceTypes volunteerDistanceType = (BO.Enums.DistanceTypes)doVolunteer.DistanceType;

            if (currentAssignment is not null)
            {
                DO.Call? callDetails;
                lock (AdminManager.BlMutex)
                {
                    callDetails = s_dal.Call.Read(currentAssignment.CallId);
                }

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
                        Max_finish_time = callDetails.MaxTimeForClosing ?? DateTime.Now,
                        Start_time = currentAssignment.EntryTimeForTreatment,
                        CallDistance = await Tools.CalculateDistanceAsync(
            volunteerDistanceType,
            doVolunteer.Latitude ?? 0,
            doVolunteer.Longitude ?? 0,
            callDetails.Latitude,
            callDetails.Longitude),
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
                CallInProgress = callInProgress
            };
        }
        catch (DO.DalDoesNotExistException)
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
        IEnumerable<DO.Assignment> currentVolunteerAssignments;
        lock (AdminManager.BlMutex)
        {
            currentVolunteerAssignments = s_dal.Assignment.ReadAll(a => a?.VolunteerId == doVolunteer.ID);
        }

        var totalHandled = currentVolunteerAssignments.Count(a => a?.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.Treated);
        var totalCanceled = currentVolunteerAssignments.Count(a => a?.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.ManagerCancellation || a!.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.SelfCancellation);
        var totalExpired = currentVolunteerAssignments.Count(a => a?.TypeOfFinishTreatment == DO.TypeOfFinishTreatment.OutOfRangeCancellation);
        var assignedCallId = currentVolunteerAssignments.FirstOrDefault(a => a?.EndTimeForTreatment == null)?.CallId;

        BO.Enums.CallType callType = BO.Enums.CallType.none;
        if (assignedCallId is not null)
        {
            lock (AdminManager.BlMutex)
            {
                callType = (BO.Enums.CallType)(s_dal.Call.Read(assignedCallId.Value)?.TypeOfCall ?? DO.TypeOfCall.ToPackageFood);
            }
        }

        return new BO.VolunteerInList
        {
            Id = doVolunteer.ID,
            FullName = doVolunteer.Name,
            IsActive = doVolunteer.IsActive,
            TotalHandledCalls = totalHandled,
            TotalCanceledCalls = totalCanceled,
            TotalExpiredCalls = totalExpired,
            CallId = assignedCallId,
            CallType = callType
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
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
    }

    internal static string GenerateStrongPassword()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#$%^&*";
        return new string(Enumerable.Repeat(chars, 12)
                               .Select(s => s[random.Next(s.Length)])
                               .ToArray());
    }

    public static bool IsValidId(int id)
    {
        string idStr = id.ToString().PadLeft(9, '0');

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

        if (volunteer.Password != null)
        {
            if (volunteer.Password!.Length > 12 ||
             !Regex.IsMatch(volunteer.Password, "[A-Z]") ||
             !Regex.IsMatch(volunteer.Password, "[a-z]") ||
             !Regex.IsMatch(volunteer.Password, "[0-9]") ||
             !Regex.IsMatch(volunteer.Password, "[!@#$%^&=*]"))
                throw new BO.BlInvalidFormatException("Invalid password you must contain 12 characters -at list: one capital letter one letter one number and one speical character!");
        }

        if (volunteer.Role != BO.Enums.Role.volunteer && volunteer.Role != BO.Enums.Role.manager)
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
        if (password.Length < 8) return false;
        if (!password.Any(char.IsUpper)) return false;
        if (!password.Any(char.IsLower)) return false;
        if (!password.Any(char.IsDigit)) return false;
        if (!password.Any(c => "@#$%^&*".Contains(c))) return false;
        return true;
    }

    private static readonly Random s_rand = new();
    private static int s_simulatorCounter = 0;

    internal static void SimulateAssigningCallsToVolunteers()
    {
        Thread.CurrentThread.Name = $"Simulator{++s_simulatorCounter}";

        List<DO.Volunteer?> doVolunteerList;
        lock (AdminManager.BlMutex)
            doVolunteerList = s_dal.Volunteer.ReadAll(v => v?.IsActive == true).ToList();

        foreach (var doVolunteer in doVolunteerList)
        {
            if (doVolunteer == null) continue;
            int volunteerId = doVolunteer.ID;
            DO.Assignment? currentAssignment;
            lock (AdminManager.BlMutex)
                currentAssignment = s_dal.Assignment.ReadAll(a => a?.VolunteerId == volunteerId && a.EndTimeForTreatment == null && a?.EntryTimeForTreatment != null).LastOrDefault();

            if (currentAssignment == null)
            {
                if (s_rand.Next(100) < 20)
                {
                    IEnumerable<BO.OpenCallInList> openCalls;
                    // If you have only async version, use: openCalls = await s_bl.Call.GetOpenCallsForVolunteerAsync(volunteerId).Result;
                    lock (AdminManager.BlMutex)
                        openCalls = s_bl.Call.GetOpenCallsForVolunteerAsync(volunteerId).GetAwaiter().GetResult();

                    var callsWithCoordinates = openCalls.Where(c => c?.CallDistance != null).ToList();
                    if (callsWithCoordinates.Count > 0)
                    {
                        var selectedCall = callsWithCoordinates[s_rand.Next(callsWithCoordinates.Count)];
                        lock (AdminManager.BlMutex)
                            s_bl.Call.SelectCallForTreatment(doVolunteer.ID, selectedCall.Id, true);
                    }
                }
            }
            else
            {
                DO.Call? call;
                lock (AdminManager.BlMutex)
                    call = s_dal.Call.Read(currentAssignment.CallId);

                if (call == null) continue;

                double? distance = Tools.CalculateDistance(
                    doVolunteer.Latitude ?? 0,
                    doVolunteer.Longitude ?? 0,
                    call.Latitude,
                    call.Longitude
                );
                if (distance == null) continue;

                TimeSpan timePassed;
                lock (AdminManager.BlMutex)
                    timePassed = s_bl.Admin.GetClock() - currentAssignment.EntryTimeForTreatment;

                double rawMinutes = distance.Value * 1.5 + s_rand.Next(2, 6);

                if (rawMinutes <= 0 || rawMinutes > 1_000_000)
                {
                    rawMinutes = 10;
                }

                TimeSpan minRequiredTime = TimeSpan.FromMinutes(rawMinutes);

                if (timePassed >= minRequiredTime)
                {
                    lock (AdminManager.BlMutex)
                        s_bl.Call.MarkCallCompletion(volunteerId, currentAssignment.ID, true);
                }
                else if (s_rand.Next(100) < 10)
                {
                    lock (AdminManager.BlMutex)
                        s_bl.Call.MarkCallCancellation(volunteerId, currentAssignment.ID, true);
                }
            }
        }
    }
}