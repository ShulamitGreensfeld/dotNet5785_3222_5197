using BlApi;
using BO;
using DO;

namespace BlTest
{
    internal class Program
    {
        static readonly IBl s_bl = BlApi.Factory.Get();
        static int nextCallId = 1100;
        static void Main(string[] args)
        {
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("Main Menu:");
                Console.WriteLine("1. Admin");
                Console.WriteLine("2. Call");
                Console.WriteLine("3. Volunteer");
                Console.WriteLine("4. Exit");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        HandleAdminMenu();
                        break;
                    case "2":
                        HandleCallMenu();
                        break;
                    case "3":
                        HandleVolunteerMenu();
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }




        //ADMIN
        private static void HandleAdminMenu()
        {
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("Admin Menu:");
                Console.WriteLine("1. Get Clock");
                Console.WriteLine("2. Promote Clock");
                Console.WriteLine("3. Get Risk Time Range");
                Console.WriteLine("4. Set Risk Time Range");
                Console.WriteLine("5. Reset Database");
                Console.WriteLine("6. Initialize Database");
                Console.WriteLine("7. Back to Main Menu");
                Console.Write("Choose an option: ");
                string? choice = Console.ReadLine();
                try
                {
                    switch (choice)
                    {
                        case "1":
                            Console.WriteLine(s_bl.Admin.GetClock());
                            break;
                        case "2":
                            PromoteClock();
                            break;
                        case "3":
                            Console.WriteLine(s_bl.Admin.GetRiskTimeRange());
                            break;
                        case "4":
                            SetRiskTimeRange();
                            break;
                        case "5":
                            s_bl.Admin.ResetDatabase();
                            break;
                        case "6":
                            s_bl.Admin.InitializeDatabase();
                            break;
                        case "7":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
                }
            }
        }

        private static void PromoteClock()
        {
            Console.WriteLine("Select time unit for promotion:");
            Console.WriteLine("1. Minute");
            Console.WriteLine("2. Hour");
            Console.WriteLine("3. Day");
            Console.WriteLine("4. Month");
            Console.WriteLine("5. Year");
            Console.Write("Choose an option: ");
            string? choice = Console.ReadLine();

            BO.Enums.TimeUnit timeUnit = choice switch
            {
                "1" => BO.Enums.TimeUnit.Minute,
                "2" => BO.Enums.TimeUnit.Hour,
                "3" => BO.Enums.TimeUnit.Day,
                "4" => BO.Enums.TimeUnit.Month,
                "5" => BO.Enums.TimeUnit.Year,
                _ => BO.Enums.TimeUnit.none
            };
            if (timeUnit == BO.Enums.TimeUnit.none)
                Console.WriteLine("invalid input, try again.");
            else
                s_bl.Admin.PromoteClock(timeUnit);
        }

        private static void SetRiskTimeRange()
        {
            Console.Write("Enter the risk time range in minutes: ");
            if (int.TryParse(Console.ReadLine(), out int minutes))
            {
                TimeSpan riskTimeRange = TimeSpan.FromMinutes(minutes);
                s_bl.Admin.SetRiskTimeRange(riskTimeRange);
                Console.WriteLine($"Risk time range set to {riskTimeRange}.");
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }


        //CALL

        private static void HandleCallMenu()
        {
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("Call Menu:");
                Console.WriteLine("1. Get Call Details");
                Console.WriteLine("2. Add Call");
                Console.WriteLine("3. Update Call");
                Console.WriteLine("4. Delete Call");
                Console.WriteLine("5. Get Closed Calls Handled by Volunteer");
                Console.WriteLine("6. Get Open Calls for Volunteer");
                Console.WriteLine("7. Mark Call as Canceled");
                Console.WriteLine("8. Mark Call as Completed");
                Console.WriteLine("9. Get Call Quantities by Status");
                Console.WriteLine("10. Get Calls List");
                Console.WriteLine("11. Select Call for Treatment");
                Console.WriteLine("12. Back to Main Menu");
                Console.Write("Choose an option: ");
                string? choice = Console.ReadLine();
                try
                {
                    switch (choice)
                    {
                        case "1":
                            GetCallDetails();
                            break;
                        case "2":
                            AddCall();
                            break;
                        case "3":
                            UpdateCall();
                            break;
                        case "4":
                            DeleteCall();
                            break;
                        case "5":
                            GetClosedCallsHandledByVolunteer();
                            break;
                        case "6":
                            GetOpenCallsForVolunteer();
                            break;
                        case "7":
                            MarkCallCancellation();
                            break;
                        case "8":
                            MarkCallCompletion();
                            break;
                        case "9":
                            GetCallQuantitiesByStatus();
                            break;
                        case "10":
                            GetCallsList();
                            break;
                        case "11":
                            SelectCallForTreatment();
                            break;
                        case "12":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
                }
            }
        }

        private static void GetCallDetails()
        {
            Console.Write("Enter Call ID: ");
            if (int.TryParse(Console.ReadLine(), out int callId))
            {
                try
                {
                    BO.Call call = s_bl.Call.GetCallDetails(callId);
                    Console.WriteLine(call);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }
        private static void GetCallsList()
        {
            Console.WriteLine("Enter filter field (optional): ");
            string? fieldInput = Console.ReadLine();
            BO.Enums.CallInListFields? fieldFilter = fieldInput != "" ? Enum.TryParse(fieldInput, out BO.Enums.CallInListFields field) ? field : (BO.Enums.CallInListFields?)null : null;

            Console.WriteLine("Enter filter value (optional): ");
            string? filterValueInput = Console.ReadLine();
            // משתנה עזר שאינו Nullable
            BO.Enums.CallStatus tempFilterValue;
            // המרה תוך התחשבות בערך ריק
            BO.Enums.CallStatus? filterValue =
                Enum.TryParse(filterValueInput, out tempFilterValue) ? tempFilterValue : (BO.Enums.CallStatus?)null;

            Console.WriteLine("Enter sort field (optional): ");
            string? sortFieldInput = Console.ReadLine();
            BO.Enums.CallInListFields? sortField = sortFieldInput != "" ? Enum.TryParse(sortFieldInput, out BO.Enums.CallInListFields sort) ? sort : (BO.Enums.CallInListFields?)null : null;

            try
            {
                var calls = s_bl.Call.GetCallsList(fieldFilter, filterValue, sortField);
                foreach (var call in calls)
                {
                    Console.WriteLine(call);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
            }
        }
        private static void SelectCallForTreatment()
        {
            Console.Write("Enter Volunteer ID: ");
            if (int.TryParse(Console.ReadLine(), out int volunteerId))
            {
                Console.Write("Enter Call ID: ");
                if (int.TryParse(Console.ReadLine(), out int callId))
                {
                    try
                    {
                        s_bl.Call.SelectCallForTreatment(volunteerId, callId);
                        Console.WriteLine("Call selected for treatment.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Call ID.");
                }
            }
            else
            {
                Console.WriteLine("Invalid Volunteer ID.");
            }
        }
        private static void AddCall()
        {
            Console.Write("Enter Call Description: ");
            string? description = Console.ReadLine();
            Console.Write("Enter Call type(ToPrepareFood, ToCarryFood, ToPackageFood, ToDonateRawMaterials, ToCommunityCookingNights,): ");
            BO.Enums.CallType callType = Enum.TryParse(Console.ReadLine(), out BO.Enums.CallType parsedType) ? parsedType : throw new ArgumentException("Invalid call type.");
            Console.Write("Enter Full Address: ");
            string address = Console.ReadLine()!;

            var newCall = new BO.Call
            {
                Id = NextCallId(),
                Verbal_description = description,
                FullAddress = address,
                Opening_time = DateTime.Now,
                CallType = callType,
                CallStatus = BO.Enums.CallStatus.opened
            };
            try
            {
                s_bl.Call.AddCall(newCall);
                Console.WriteLine("Call added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
            }
        }

        private static int NextCallId()
        {
            // Implement the logic to generate the next call ID
            // This is a placeholder implementation
            //return new Random().Next(1, 1000);
            return nextCallId++;
        }

        private static void UpdateCall()
        {
            Console.Write("Enter Call ID: ");
            int.TryParse(Console.ReadLine(), out int callId);
            Console.Write("Enter New Description (optional) : ");
            string description = Console.ReadLine();
            Console.Write("Enter New Full Address (optional) : ");
            string address = Console.ReadLine();
            Console.Write("Enter Call Type (optional) : ");
            BO.Enums.CallType? callType = Enum.TryParse(Console.ReadLine(), out BO.Enums.CallType parsedType) ? parsedType : (BO.Enums.CallType?)null;
            Console.Write("Enter Max Finish Time (hh:mm , (optional)): ");
            TimeSpan? maxFinishTime = TimeSpan.TryParse(Console.ReadLine(), out TimeSpan parsedTime) ? parsedTime : (TimeSpan?)null;
            try
            {
                var callToUpdate = s_bl.Call.GetCallDetails(callId);
                if (callToUpdate == null)
                    throw new BO.BlDoesNotExistException($"Call with ID{callId} does not exist!");
                var newUpdatedCall = new BO.Call
                {
                    Id = callId,
                    Verbal_description = !string.IsNullOrWhiteSpace(description) ? description : callToUpdate.Verbal_description,
                    FullAddress = !string.IsNullOrWhiteSpace(address) ? address : /*callToUpdate.FullAddress*/"No Address",
                    Opening_time = callToUpdate.Opening_time,
                    Max_finish_time = (maxFinishTime.HasValue ? DateTime.Now.Date + maxFinishTime.Value : callToUpdate.Max_finish_time),
                    CallType = callType ?? callToUpdate.CallType
                };
                s_bl.Call.UpdateCallDetails(newUpdatedCall);
                Console.WriteLine("Call updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
            }
        }
        private static void DeleteCall()
        {
            Console.Write("Enter Call ID to delete: ");
            if (int.TryParse(Console.ReadLine(), out int callId))
            {
                try
                {
                    s_bl.Call.DeleteCall(callId);
                    Console.WriteLine("Call deleted successfully.");
                }
                catch (DO.DalDoesNotExistException ex)
                {
                    throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist", ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }
        private static void GetClosedCallsHandledByVolunteer()
        {
            Console.Write("Enter Volunteer ID: ");
            int volunteerId = int.TryParse(Console.ReadLine(), out int vId) ? vId : throw new BO.BlInvalidFormatException("Invalid format of ID!");
            try
            {
                var closedCalls = s_bl.Call.GetClosedCallsHandledByVolunteer(volunteerId);
                foreach (var call in closedCalls)
                {
                    Console.WriteLine(call);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
            }

        }
        private static void GetOpenCallsForVolunteer()
        {
            Console.Write("Enter Volunteer ID: ");
            int.TryParse(Console.ReadLine(), out int volunteerId);
            try
            {
                var openCalls = s_bl.Call.GetOpenCallsForVolunteer(volunteerId);
                foreach (var call in openCalls)
                {
                    Console.WriteLine(call);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
            }
        }
        static void GetCallQuantitiesByStatus()
        {
            var amounts = s_bl.Call.GetCallQuantitiesByStatus();
            foreach (var amount in amounts)
            {
                Console.WriteLine(amount);
            }
        }
        private static void MarkCallCancellation()
        {
            Console.Write("Enter Volunteer ID: ");
            if (int.TryParse(Console.ReadLine(), out int volunteerId))
            {
                try
                {
                    // Check if the volunteer ID is valid
                    var volunteer = s_bl.Volunteer.GetVolunteerDetails(volunteerId);
                    if (volunteer == null)
                    {
                        Console.WriteLine("Invalid Volunteer ID.");
                        return;
                    }

                    Console.Write("Enter assignment ID: ");
                    if (int.TryParse(Console.ReadLine(), out int assignmentId))
                    {
                        s_bl.Call.MarkCallCancellation(volunteerId, assignmentId);
                        Console.WriteLine("Call marked as canceled.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid Assignment ID.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid Volunteer ID.");
            }
        }
        private static void MarkCallCompletion()
        { 
            Console.Write("Enter Volunteer ID: ");
            int.TryParse(Console.ReadLine(), out int volunteerId);
            Console.Write("Enter Assignment ID: ");
            int.TryParse(Console.ReadLine(), out int assignmentId);

            try
            {
                s_bl.Call.MarkCallCompletion(volunteerId, assignmentId);
                Console.WriteLine("Call marked as completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
            }
        }

        // Volunteer
        private static void HandleVolunteerMenu()
        {
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("Volunteer Menu:");
                Console.WriteLine("1. Get Volunteer Details");
                Console.WriteLine("2. Get Volunteers List");
                Console.WriteLine("3. Add Volunteer");
                Console.WriteLine("4. Update Volunteer Details");
                Console.WriteLine("5. Delete Volunteer");
                Console.WriteLine("6. Login to System");
                Console.WriteLine("7. Back to Main Menu");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine();
                try
                {
                    switch (choice)
                    {
                        case "1":
                            GetVolunteerDetails();
                            break;
                        case "2":
                            GetVolunteersList();
                            break;
                        case "3":
                            AddVolunteer();
                            break;
                        case "4":
                            UpdateVolunteerDetails();
                            break;
                        case "5":
                            DeleteVolunteer();
                            break;
                        case "6":
                            EnterVolunteerSystem();
                            break;
                        case "7":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
                }
            }
        }

        private static void GetVolunteerDetails()
        {
            Console.Write("Enter Volunteer ID: ");
            if (int.TryParse(Console.ReadLine(), out int volunteerId))
            {
                try
                {
                    BO.Volunteer volunteer = s_bl.Volunteer.GetVolunteerDetails(volunteerId);
                    if (volunteer is null)
                        throw new BO.BlDoesNotExistException($"Volunteer with Id {volunteerId} does not exist!");
                    Console.WriteLine(volunteer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }

        private static void GetVolunteersList()
        {
            Console.WriteLine("Enter filter for active volunteers (optional, true/false): ");
            string? isActiveInput = Console.ReadLine();
            bool? isActiveFilter = isActiveInput != "" ? bool.TryParse(isActiveInput, out bool isActive) ? isActive : (bool?)null : null;

            Console.WriteLine("Enter sort field for volunteers (optional): ");
            string? sortFieldInput = Console.ReadLine();
            BO.Enums.VolunteerInListFields? sortField = sortFieldInput != "" ? Enum.TryParse(sortFieldInput, out BO.Enums.VolunteerInListFields field) ? field : (BO.Enums.VolunteerInListFields?)null : null;

            try
            {
                var volunteers = s_bl.Volunteer.GetVolunteersList(isActiveFilter, sortField);
                foreach (var volunteer in volunteers)
                {
                    // Exclude Call ID and Call Type from the output
                    Console.WriteLine($"ID: {volunteer.Id}, Name: {volunteer.FullName}, IsActive: {volunteer.IsActive}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
            }
        }

        private static void AddVolunteer()
        {
            Console.WriteLine("Enter Volunteer details:");
            Console.WriteLine("ID:");
            int.TryParse(Console.ReadLine(), out int volunteerId);
            
           Console.Write("Full Name: ");
            string name = Console.ReadLine();
            Console.WriteLine("Cellphone Number:");
            string cellphone = Console.ReadLine();
            Console.WriteLine("Email:");
            string email = Console.ReadLine();
            Console.WriteLine("Password (optional):");
            string pass = Console.ReadLine();
            Console.Write("Address (optional): ");
            string address = Console.ReadLine();
            Console.WriteLine("Role (volunteer/manager):");
            BO.Enums.Role.TryParse(Console.ReadLine(), out BO.Enums.Role role);
            Console.WriteLine("IsActive (true/false):");
            bool.TryParse(Console.ReadLine(), out bool isActive);
            Console.WriteLine("Distance Type (aerial_distance, walking_distance, driving_distance):");
            BO.Enums.DistanceTypes.TryParse(Console.ReadLine(), out BO.Enums.DistanceTypes distanceType);
            Console.WriteLine("Max Distance (optional):");
            double.TryParse(Console.ReadLine(), out double maxDistance);
            BO.Volunteer newVolunteer = new BO.Volunteer()
            {
                Id = volunteerId,
                FullName = name,
                CellphoneNumber = cellphone,
                Email = email,
                Password = pass,
                FullAddress = address,
                Role = role,
                IsActive = isActive,
                DistanceType = distanceType,
                MaxDistance = maxDistance
            };

            try
            {
                s_bl.Volunteer.AddVolunteer(newVolunteer);
                Console.WriteLine("Volunteer added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
            }
        }

        private static void UpdateVolunteerDetails()
        {
            Console.Write("Enter Requester ID: ");
            int requesterId = int.Parse(Console.ReadLine());

            Console.Write("Enter Volunteer ID to Update: ");
            int volunteerId = int.Parse(Console.ReadLine());

            Console.Write("Enter Volunteer Password: ");
            string volunteerPassword = Console.ReadLine();

            var doVolunteer = s_bl.Volunteer.GetVolunteerDetails(volunteerId);
            if (doVolunteer is null)
                throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist!");

            Console.Write("Full Name(optional): ");
            string name = Console.ReadLine();

            Console.WriteLine("Cellphone Number(optional):");
            string cellphone = Console.ReadLine();

            Console.WriteLine("Email(optional):");
            string email = Console.ReadLine();

            Console.WriteLine("Password (optional):");
            string pass = Console.ReadLine();

            Console.Write("Address (optional): ");
            string address = Console.ReadLine();

            Console.WriteLine("Role (volunteer/manager)(optional):");
            string roleInput = Console.ReadLine()!;
            BO.Enums.Role role = string.IsNullOrEmpty(roleInput) || !Enum.TryParse(roleInput, out BO.Enums.Role rType) ? doVolunteer.Role : rType;

            Console.WriteLine("IsActive (true/false)(optional):");
            string isActiveInput = Console.ReadLine()!;
            bool isActive = string.IsNullOrEmpty(isActiveInput) || !bool.TryParse(isActiveInput, out bool isA) ? doVolunteer.IsActive : isA;

            Console.WriteLine("Distance Type (aerial_distance, walking_distance, driving_distance)(optional):");
            string distanceTypeInput = Console.ReadLine()!;
            BO.Enums.DistanceTypes distanceType = string.IsNullOrEmpty(distanceTypeInput) || !Enum.TryParse(distanceTypeInput, out BO.Enums.DistanceTypes dType) ? doVolunteer.DistanceType : dType;

            Console.WriteLine("Max Distance (optional):");
            string maxDistanceInput = Console.ReadLine()!;
            double? maxDistance = string.IsNullOrEmpty(maxDistanceInput) || !double.TryParse(maxDistanceInput, out double maxDis) ? doVolunteer.MaxDistance : maxDis;

            BO.Volunteer updatedVolunteer = new BO.Volunteer()
            {
                Id = volunteerId,
                FullName = string.IsNullOrEmpty(name) ? doVolunteer.FullName : name,
                CellphoneNumber = string.IsNullOrEmpty(cellphone) ? doVolunteer.CellphoneNumber : cellphone,
                Email = string.IsNullOrEmpty(email) ? doVolunteer.Email : email,
                Password = pass,
                FullAddress = string.IsNullOrEmpty(address) ? doVolunteer.FullAddress : address,
                Role = role,
                IsActive = isActive,
                DistanceType = distanceType,
                MaxDistance = maxDistance
            };

            try
            {
                s_bl.Volunteer.UpdateVolunteerDetails(requesterId, updatedVolunteer);
                Console.WriteLine("Volunteer details updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
            }
        }


        private static void DeleteVolunteer()
        {
            try
            {
                Console.Write("Enter Volunteer ID to Delete: ");
                if (int.TryParse(Console.ReadLine(), out int volunteerId))
                    s_bl.Volunteer.DeleteVolunteer(volunteerId);
                Console.WriteLine("Volunteer deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
            }
        }

        private static void EnterVolunteerSystem()
        {
            try
            {
                Console.Write("Enter Volunteer Name : ");
                string volunteerName = Console.ReadLine() ?? throw new BO.BlNullPropertyException("id can't be null");
                Console.Write("Enter Volunteer Password: ");
                string volunteerPass = Console.ReadLine() ?? throw new BO.BlNullPropertyException("password can't be null");
                s_bl.Volunteer.EnterSystem(volunteerName, volunteerPass);
                Console.WriteLine("You've logged in successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
            }
        }
    }
}


//private static void GetVolunteersList()
//{
//    Console.WriteLine("Enter filter for active volunteers (optional, true/false): ");
//    string? isActiveInput = Console.ReadLine();
//    bool? isActiveFilter = isActiveInput != "" ? bool.TryParse(isActiveInput, out bool isActive) ? isActive : (bool?)null : null;

//    Console.WriteLine("Enter sort field for volunteers (optional): ");
//    string? sortFieldInput = Console.ReadLine();
//    BO.Enums.VolunteerInListFields? sortField = sortFieldInput != "" ? Enum.TryParse(sortFieldInput, out BO.Enums.VolunteerInListFields field) ? field : (BO.Enums.VolunteerInListFields?)null : null;

//    try
//    {
//        var volunteers = s_bl.Volunteer.GetVolunteersList(isActiveFilter, sortField);
//        foreach (var volunteer in volunteers)
//        {
//            Console.WriteLine(volunteer);
//        }
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
//    }
//}

//private static void AddVolunteer()
//{
//    Console.WriteLine("Enter Volunteer details:");
//    BO.Volunteer newVolunteer = new BO.Volunteer();

//    // Collect volunteer details from user
//    Console.Write("Full Name: ");
//    newVolunteer.FullName = Console.ReadLine();

//    Console.Write("Address: ");
//    newVolunteer.FullAddress = Console.ReadLine();

//    // You can add other necessary fields here (like Phone Number, Role, etc.)

//    try
//    {
//        s_bl.Volunteer.AddVolunteer(newVolunteer);
//        Console.WriteLine("Volunteer added successfully.");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
//    }
//}

//private static void UpdateVolunteerDetails()
//{
//    Console.Write("Enter Requester ID: ");
//    int requesterId = int.Parse(Console.ReadLine());

//    Console.Write("Enter Volunteer ID to Update: ");
//    int volunteerId = int.Parse(Console.ReadLine());

//    Console.WriteLine("Enter new details for Volunteer:");
//    BO.Volunteer updatedVolunteer = new BO.Volunteer();

//    // Collect updated volunteer details from user
//    Console.Write("Full Name: ");
//    updatedVolunteer.FullName = Console.ReadLine();

//    Console.Write("Address: ");
//    updatedVolunteer.FullAddress = Console.ReadLine();

//    // You can add other fields here (like Phone Number, Role, etc.)

//    try
//    {
//        s_bl.Volunteer.UpdateVolunteerDetails(requesterId, updatedVolunteer);
//        Console.WriteLine("Volunteer details updated successfully.");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
//    }
//}

//private static void DeleteVolunteer()
//{
//    Console.Write("Enter Volunteer ID to Delete: ");
//    int volunteerId = int.Parse(Console.ReadLine());

//    try
//    {
//        s_bl.Volunteer.DeleteVolunteer(volunteerId);
//        Console.WriteLine("Volunteer deleted successfully.");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
//    }
//}


//private static void EnterVolunteerSystem()
//{
//    Console.Write("Enter Volunteer Name: ");
//    string name = Console.ReadLine();

//    Console.Write("Enter Password: ");
//    string password = Console.ReadLine();

//    try
//    {
//        BO.Enums.Role role = s_bl.Volunteer.EnterSystem(name, password);
//        Console.WriteLine($"Welcome, {name}. Your role is: {role}");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
//    }
//}


//private static void EnterVolunteerSystem()
//{
//    Console.Write("Enter Volunteer ID (Tz): ");
//    string tz = Console.ReadLine();

//    try
//    {
//        // המערכת תוודא שהמתנדב קיים בעזרת תעודת הזהות
//        int volunteerId = s_bl.Volunteer.EnterSystemByTz(tz);
//        Console.WriteLine($"Volunteer found! ID: {volunteerId}");

//        // לאחר שהמתנדב נמצא, נבקש את פרטי המתנדב
//        GetVolunteerDetailsAfterLogin(volunteerId);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
//    }
//}
