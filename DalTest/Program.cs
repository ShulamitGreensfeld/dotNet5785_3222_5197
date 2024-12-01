using Dal;
using DalApi;
using DO;

namespace DalTest
{
    public static class Program
    {
        private static ICall? s_dalCall = new CallImplementation();
        private static IVolunteer? s_dalVolunteer = new VolunteerImplementation();
        private static IAssignment? s_dalAssignment = new AssignmentImplementation();
        private static IConfig? s_dalConfig = new ConfigImplementation();

        public static void Main()
        {
            try
            {
                bool isRunning = true;
                do
                {
                    PrintMainMenu();
                    int choice = TryGetValidChoice();

                    switch (choice)
                    {
                        case 0: isRunning = false; break;
                        case 1: AssignmentsMenu(); break;
                        case 2: CallsMenu(); break;
                        case 3: VolunteersMenu(); break;
                        case 4: ConfigurationMenu(); break;
                        case 5: InitializeDatabase(); break;
                        case 6: DisplayAllData(); break;
                        case 7: ResetDatabase(); break;
                        default: Console.WriteLine("Invalid choice. Please try again."); break;
                    }
                } while (isRunning);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static int TryGetValidChoice()
        {
            int choice;
            while (!int.TryParse(Console.ReadLine(), out choice))
                Console.WriteLine("Invalid input. Please enter a valid number.");
            return choice;
        }

        private static void PrintMainMenu()
        {
            Console.WriteLine("\nMain Menu:");
            Console.WriteLine("0. Exit");
            Console.WriteLine("1. Assignments Menu");
            Console.WriteLine("2. Calls Menu");
            Console.WriteLine("3. Volunteers Menu");
            Console.WriteLine("4. Configuration Menu");
            Console.WriteLine("5. Initialize Database");
            Console.WriteLine("6. Display All Data");
            Console.WriteLine("7. Reset Database");
            Console.Write("Enter your choice: ");
        }

        private static void ResetDatabase()
        {
            Console.Clear();
            Console.WriteLine("Are you sure you want to reset the database? Press 'yes' to confirm.");
            string? confirmation = Console.ReadLine();
            if (confirmation?.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase) == true)
            {
                try
                {
                    s_dalCall!.DeleteAll();
                    s_dalVolunteer!.DeleteAll();
                    s_dalAssignment!.DeleteAll();
                    s_dalConfig!.Reset();
                    Console.WriteLine("Database and configuration reset successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error resetting database: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Database reset canceled.");
            }
        }

        private static void InitializeDatabase()
        {
            try
            {
                Initialization.Do(s_dalCall, s_dalAssignment, s_dalVolunteer, s_dalConfig);
                Console.WriteLine("Database initialized successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Initialization failed: {ex.Message}");
            }
        }

        private static void DisplayAllData()
        {
            try
            {
                Console.WriteLine("\t\t\t\t\t--------Calls Start--------");
                ReadAllCalls();
                Console.WriteLine("\t\t\t\t\t--------Calls End--------");
                Console.WriteLine("\t\t\t\t\t--------Volunteers Start--------");
                ReadAllVolunteers();
                Console.WriteLine("\t\t\t\t\t--------Volunteers End--------");
                Console.WriteLine("\t\t\t\t\t--------Assignments Start--------");
                ReadAllAssignments();
                Console.WriteLine("\t\t\t\t\t--------Assignments End--------");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying data: {ex.Message}");
            }
        }

        private static void AssignmentsMenu()
        {
            Console.Clear();
            Console.WriteLine("\nAssignment Menu:");
            Console.WriteLine("0. Back to Main Menu");
            Console.WriteLine("1. Create Assignment");
            Console.WriteLine("2. Read Assignment by ID");
            Console.WriteLine("3. Read All Assignments");
            Console.WriteLine("4. Update Assignment");
            Console.WriteLine("5. Delete Assignment by ID");
            Console.WriteLine("6. Delete All Assignments");

            bool isRunning = true;
            do
            {
                try
                {
                    Console.Write("Enter your choice: ");
                    int choice = TryGetValidChoice();

                    switch (choice)
                    {
                        case 0: isRunning = false; break;
                        case 1: CreateAssignment(); break;
                        case 2: ReadAssignment(); break;
                        case 3: ReadAllAssignments(); break;
                        case 4: UpdateAssignment(); break;
                        case 5: DeleteAssignment(); break;
                        case 6: DeleteAllAssignments(); break;
                        case 7: ResetDatabase(); break;
                        default: Console.WriteLine("Invalid choice. Please try again."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            } while (isRunning);
        }

        private static void CreateAssignment()
        {
            Console.WriteLine("Enter Volunteer ID: ");
            if (!int.TryParse(Console.ReadLine()!, out int volunteerId) || (s_dalVolunteer!.Read(volunteerId) == null))
                throw new FormatException("Volunteer ID is invalid!");
            Console.WriteLine("Enter Call ID: ");
            if (!int.TryParse(Console.ReadLine()!, out int callId) || (s_dalCall!.Read(callId) == null))
                throw new FormatException("Call ID is invalid!");
            Assignment newAssignment = new() { VolunteerId = volunteerId, CallId = callId, };
            s_dalAssignment!.Create(newAssignment);
            Console.WriteLine("An Assignment was successfully created.");
        }

        private static void ReadAssignment()
        {
            Console.Write("Enter Assignment ID to read: ");
            int id = int.Parse(Console.ReadLine()!);
            var assignment = s_dalAssignment?.Read(id);
            if (assignment != null)
            {
                Console.WriteLine($"Assignment ID: {assignment.ID}, Volunteer ID: {assignment.VolunteerId}, " +
                    $"Call Id: {assignment.CallId},  Start Time: {assignment.EntryTimeForTreatment}, " +
                    $"End Time: {assignment.EndTimeForTreatment}, End Type: {assignment.TypeOfFinishTreatment}, ");
            }
            else
                Console.WriteLine("Assignment not found.");
        }

        private static void ReadAllAssignments()
        {
            var assignments = s_dalAssignment?.ReadAll();
            if (assignments != null)
            {
                foreach (var assignment in assignments)
                {
                    Console.WriteLine($"Assignment ID: {assignment.ID}, Volunteer ID: {assignment.VolunteerId}, " +
                        $"Call Id: {assignment.CallId},  Start Time: {assignment.EntryTimeForTreatment}, " +
                        $"End Time: {assignment.EndTimeForTreatment}, End Type: {assignment.TypeOfFinishTreatment}, ");
                }
            }
            else
                Console.WriteLine("No assignments found.");
        }

        private static void UpdateAssignment()
        {

            bool AskUserIfUpdate(string fieldName)
            {
                Console.WriteLine($"Do you want to update {fieldName}? (y/n): ");
                string response = Console.ReadLine()?.Trim().ToLower()!;
                return response == "y" || response == "yes";
            }


            string GetValidInput(string prompt)
            {
                string? input;
                do
                {
                    Console.Write(prompt);
                    input = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(input))
                        Console.WriteLine("Input cannot be empty. Please try again.");
                } while (string.IsNullOrEmpty(input));

                return input!;
            }
            Console.Write("Enter Assignment ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
                throw new FormatException("Call ID is invalid!");
            var assignment = s_dalAssignment?.Read(id);
            if (assignment != null)
            {
                Assignment newAssignment = new()
                {
                    ID = assignment.ID,
                    VolunteerId = AskUserIfUpdate("Volunteer ID") && int.TryParse(GetValidInput("Enter volunteer ID: "), out int parsedVolunteerId)
        ? parsedVolunteerId : assignment.VolunteerId,
                    CallId = AskUserIfUpdate("Call ID") && int.TryParse(GetValidInput("Enter call ID: "), out int parsedCallId)
        ? parsedCallId : assignment.CallId,
                    EntryTimeForTreatment = AskUserIfUpdate("Start Time") && DateTime.TryParse(GetValidInput("Enter Start Time (dd/mm/yy hh:mm:ss): "), out DateTime parsedStartTime)
        ? parsedStartTime : assignment.EntryTimeForTreatment,
                    EndTimeForTreatment = AskUserIfUpdate("End Time") && DateTime.TryParse(GetValidInput("Enter End Time (dd/mm/yy hh:mm:ss): "), out DateTime parsedEndTime)
        ? parsedEndTime : assignment.EndTimeForTreatment,
                    TypeOfFinishTreatment = AskUserIfUpdate("End Type") && Enum.TryParse(GetValidInput("Enter End Type ( Treated(0),  SelfCancellation(1),  ManagerCancellation(2),  OutOfRangeCancellation(3)): "), out DO.TypeOfFinishTreatment parsedEndType)
        ? parsedEndType : assignment.TypeOfFinishTreatment
                };

                s_dalAssignment?.Update(newAssignment);
                Console.WriteLine("Assignment updated successfully.");
                Console.WriteLine(newAssignment);
            }
            else
                Console.WriteLine("Assignment not found.");
        }
        private static void DeleteAssignment()
        {
            Console.Write("Enter Assignment ID to delete: ");
            int id = int.Parse(Console.ReadLine()!);
            s_dalAssignment?.Delete(id);
            Console.WriteLine("Call deleted successfully.");
        }

        private static void DeleteAllAssignments()
        {
            s_dalAssignment!.DeleteAll();
            Console.WriteLine("All assignments deleted successfully!");
        }

        private static void CallsMenu()
        {
            Console.Clear();
            Console.WriteLine("\nCall Menu:");
            Console.WriteLine("0. Back to Main Menu");
            Console.WriteLine("1. Create Call");
            Console.WriteLine("2. Read Call by ID");
            Console.WriteLine("3. Read All Calls");
            Console.WriteLine("4. Update Call");
            Console.WriteLine("5. Delete Call by ID");
            Console.WriteLine("6. Delete All Calls");

            bool isRunning = true;

            do
            {
                try
                {
                    Console.Write("Enter your choice: ");
                    int choice = TryGetValidChoice();
                    switch (choice)
                    {
                        case 0: isRunning = false; break;
                        case 1: CreateCall(); break;
                        case 2: ReadCall(); break;
                        case 3: ReadAllCalls(); break;
                        case 4: UpdateCall(); break;
                        case 5: DeleteCall(); break;
                        case 6: DeleteAllCalls(); break;
                        default: Console.WriteLine("Invalid choice. Please try again."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            } while (isRunning);
        }

        private static void CreateCall()
        {
            Console.Write("Enter Call Type ( ToPrepareFood(0),   ToCarryFood(1),  ToPackageFood(2),   ToDonateRawMaterials(3),   ToCommunityCookingNights(4)): ");
            if (!Enum.TryParse(Console.ReadLine()!, out DO.TypeOfCall call_type))
                throw new FormatException("Call Type is invalid!");
            Console.Write("Enter Call Description: ");
            string verbal_description = Console.ReadLine()!;
            Console.Write("Enter Full Address: ");
            string full_address = Console.ReadLine()!;
            Console.Write("Enter Latitude: ");
            if (!double.TryParse(Console.ReadLine(), out double latitude))
                throw new FormatException("Latitude is invalid!");
            Console.Write("Enter Longitude: ");
            if (!double.TryParse(Console.ReadLine(), out double longitude))
                throw new FormatException("Longitude is invalid!");
            Console.Write("Enter full latest end time(dd/mm/yy hh:mm:ss): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime endTime))
                throw new FormatException("End time is invalid!");
            Call newCall = new()
            {
                TypeOfCall = call_type,
                CallDescription = verbal_description,
                Address = full_address,
                Latitude = latitude,
                Longitude = longitude,
                MaxTimeForClosing = endTime
            };
            s_dalCall?.Create(newCall);
            Console.WriteLine("Call created successfully.");
        }

        private static void ReadCall()
        {
            Console.Write("Enter Call ID to read: ");
            int id = int.Parse(Console.ReadLine()!);
            var call = s_dalCall?.Read(id);
            if (call != null)
            {
                Console.WriteLine($"Call ID: {call.ID}, Call Type: {call.TypeOfCall}, " +
                    $"Description: {call.CallDescription ?? "N/A"}, Full Address: {call.Address}, " +
                    $"Latitude: {call.Latitude}, Longitude: {call.Longitude}, " +
                    $"Opening Time: {call.OpeningTime}, Max Finish Time: {call.MaxTimeForClosing}");
            }
            else
                Console.WriteLine("Call not found.");
        }

        private static void ReadAllCalls()
        {
            var calls = s_dalCall?.ReadAll();
            if (calls != null)
                foreach (var call in calls)
                {
                    Console.WriteLine($"Call ID: {call.ID}, Call Type: {call.TypeOfCall}, " +
                        $"Description: {call.CallDescription ?? "N/A"}, Full Address: {call.Address}, " +
                        $"Latitude: {call.Latitude}, Longitude: {call.Longitude}, " +
                        $"Opening Time: {call.OpeningTime}, Max Finish Time: {call.MaxTimeForClosing}");
                }
            else
                Console.WriteLine("No calls found.");
        }

        private static void UpdateCall()
        {
            Console.Write("Enter Call ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
                throw new FormatException("Call ID is invalid!");

            var call = s_dalCall?.Read(id);
            if (call == null)
            {
                Console.WriteLine("Call not found.");
                return;
            }

            Console.WriteLine(call);

            bool AskUserIfUpdate(string fieldName)
            {
                Console.WriteLine($"Do you want to update {fieldName}? (yes/no): ");
                string? response = Console.ReadLine()?.Trim().ToLower();
                return response == "yes" || response == "y";
            }

            string GetValidInput(string prompt)
            {
                string? input;
                do
                {
                    Console.Write(prompt);
                    input = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(input))
                        Console.WriteLine("Input cannot be empty. Please try again.");
                } while (string.IsNullOrEmpty(input));

                return input!;
            }

            Call newCall = new()
            {
                ID = call.ID,
                TypeOfCall = AskUserIfUpdate("Call Type")
                    && Enum.TryParse(GetValidInput("Enter Call Type (ToPrepareFood(0),   ToCarryFood(1),  ToPackageFood(2),  ToDonateRawMaterials(3),   ToCommunityCookingNights(4)): "), out DO.TypeOfCall cType)
                    ? cType : call.TypeOfCall,
                CallDescription = AskUserIfUpdate("Call Description")
                    ? GetValidInput("Enter Call Description: ")
                    : call.CallDescription,
                Address = AskUserIfUpdate("Full Address")
                    ? GetValidInput("Enter Full Address: ")
                    : call.Address,
                Latitude = AskUserIfUpdate("Latitude")
                    && double.TryParse(GetValidInput("Enter Latitude: "), out double latitude)
                    ? latitude : call.Latitude,
                Longitude = AskUserIfUpdate("Longitude")
                    && double.TryParse(GetValidInput("Enter Longitude: "), out double longitude)
                    ? longitude : call.Longitude,
            };

            s_dalCall?.Update(newCall);
            Console.WriteLine("Call updated successfully.");
            Console.WriteLine(newCall);
        }

        private static void DeleteCall()
        {
            Console.Write("Enter Call ID to delete: ");
            int id = int.Parse(Console.ReadLine()!);
            s_dalCall?.Delete(id);
            Console.WriteLine("Call deleted successfully.");
        }

        private static void DeleteAllCalls()
        {
            s_dalCall!.DeleteAll();
            Console.WriteLine("All calls deleted successfully!");
        }

        private static void VolunteersMenu()
        {
            Console.Clear();
            Console.WriteLine("\nVolunteer Menu:");
            Console.WriteLine("0. Back to Main Menu");
            Console.WriteLine("1. Create Volunteer");
            Console.WriteLine("2. Read Volunteer by ID");
            Console.WriteLine("3. Read All Volunteers");
            Console.WriteLine("4. Update Volunteer");
            Console.WriteLine("5. Delete Volunteer by ID");
            Console.WriteLine("6. Delete All Volunteers");

            bool isRunning = true;

            do
            {
                try
                {
                    Console.Write("Enter your choice: ");
                    int choice = TryGetValidChoice();
                    switch (choice)
                    {
                        case 0: isRunning = false; break;
                        case 1: CreateVolunteer(); break;
                        case 2: ReadVolunteer(); break;
                        case 3: ReadAllVolunteers(); break;
                        case 4: UpdateVolunteer(); break;
                        case 5: DeleteVolunteer(); break;
                        case 6: DeleteAllVolunteers(); break;
                        default: Console.WriteLine("Invalid choice. Please try again."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            } while (isRunning);
        }

        private static void CreateVolunteer()
        {
            Console.Write("Enter Volunteer ID: ");
            if (!int.TryParse(Console.ReadLine(), out int Id))
                throw new FormatException("ID is invalid!");
            Console.Write("Enter Phone Number: ");
            string phone = Console.ReadLine()!;
            if (string.IsNullOrWhiteSpace(phone) || phone.Length < 10)
                throw new FormatException("Phone number is invalid!");
            Console.Write("Enter Name: ");
            string name = Console.ReadLine()!;
            if (string.IsNullOrWhiteSpace(name))
                throw new FormatException("Name is required!");
            Console.Write("Enter Email: ");
            string EmailAddress = Console.ReadLine()!;
            Console.Write("Enter Address: ");
            string? fullAddress = Console.ReadLine()!;
            Console.Write("Enter Latitude: ");
            if (!double.TryParse(Console.ReadLine(), out double latitude))
                throw new FormatException("Latitude is invalid!");
            Console.Write("Enter Longitude: ");
            if (!double.TryParse(Console.ReadLine(), out double longitude))
                throw new FormatException("Longitude is invalid!");
            Console.Write("Enter Role (Manager/Volunteer): ");
            if (!Enum.TryParse(Console.ReadLine(), out Role role))
                throw new FormatException("Role is invalid!");
            Console.Write("Enter if Is Active (True/False): ");
            if (!bool.TryParse(Console.ReadLine(), out bool isActive))
                throw new FormatException("IsActive is invalid!");
            Console.Write("Enter Distance Type (AirDistance (0),WalkingDistance (1),DrivingDistance (2): ");
            if (!Enum.TryParse(Console.ReadLine(), out DistanceType distanceType))
                throw new FormatException("DistanceType is invalid!");
            Console.Write("Enter Max Distance (km): ");
            double? maxDistance = null;
            string MaxDistanceInput = Console.ReadLine()!;
            if (!double.TryParse(MaxDistanceInput, out double parsedMaxDistance))
                throw new FormatException("Max Distance is invalid!");
            maxDistance = parsedMaxDistance;
            Console.Write("Enter Password (optional): ");
            string? password = Console.ReadLine();
            Volunteer newVolunteer;
            if (password == "")
                newVolunteer = new Volunteer
                {
                    ID = Id,
                    Name = name,
                    Phone = phone,
                    Email = EmailAddress,
                    Address = fullAddress,
                    Latitude = latitude,
                    Longitude = longitude,
                    Role = role,
                    IsActive = isActive,
                    DistanceType = distanceType,
                    MaxDistanceForCall = maxDistance,
                };
            else
                newVolunteer = new Volunteer
                {
                    ID = Id,
                    Name = name,
                    Phone = phone,
                    Email = EmailAddress,
                    Address = fullAddress,
                    Latitude = latitude,
                    Longitude = longitude,
                    Role = role,
                    IsActive = isActive,
                    DistanceType = distanceType,
                    MaxDistanceForCall = maxDistance,
                    Password = password,
                };
            s_dalVolunteer?.Create(newVolunteer);
            Console.WriteLine("Volunteer created successfully.");
        }


        private static void ReadVolunteer()
        {
            Console.WriteLine("Enter Volunteer ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Volunteer ID. Please enter a valid number.");
                return;
            }
            var volunteer = s_dalVolunteer?.Read(id);
            if (volunteer != null)
            {
                Console.WriteLine($"Volunteer ID: {volunteer.ID}, Full Name: {volunteer.Name}, Cellphone: {volunteer.Phone}, Email: {volunteer.Email}, " +
                    $"Full Address: {volunteer.Address}, Latitude: {volunteer.Latitude}, Longitude: {volunteer.Longitude}, " +
                    $"Role: {volunteer.Role}, Active: {volunteer.IsActive}, Distance Type: {volunteer.DistanceType}, " +
                    $"Max Distance: {volunteer.MaxDistanceForCall}, Password: {volunteer.Password}");
            }
            else
                Console.WriteLine("Volunteer not found.");
        }

        private static void ReadAllVolunteers()
        {
            var volunteers = s_dalVolunteer?.ReadAll();
            if (volunteers != null)
            {
                int i = 1;
                foreach (var volunteer in volunteers)
                {
                    Console.WriteLine($"volunteer{i++}");
                    Console.WriteLine($"Volunteer ID: {volunteer.ID}, Full Name: {volunteer.Name}, Cellphone: {volunteer.Phone}, Email: {volunteer.Email}, " +
                    $"Full Address: {volunteer.Address}, Latitude: {volunteer.Latitude}, Longitude: {volunteer.Longitude}, " +
                    $"Role: {volunteer.Role}, Active: {volunteer.IsActive}, Distance Type: {volunteer.DistanceType}, " +
                    $"Max Distance: {volunteer.MaxDistanceForCall}, Password: {volunteer.Password}");
                }
            }
            else
                Console.WriteLine("No volunteers found.");
        }

        private static void UpdateVolunteer()
        {
            Console.Write("Enter Volunteer ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
                throw new FormatException("Volunteer ID is invalid!");

            var volunteer = s_dalVolunteer?.Read(id);
            Console.WriteLine(volunteer);

            bool AskUserIfUpdate(string fieldName)
            {
                Console.WriteLine($"Do you want to update {fieldName}? (yes/no): ");
                string? response = Console.ReadLine()?.Trim().ToLower();
                return response == "yes" || response == "y" || response == "0";
            }

            string GetValidInput(string prompt)
            {
                string? input;
                do
                {
                    Console.Write(prompt);
                    input = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(input))
                        Console.WriteLine("Input cannot be empty. Please try again.");
                } while (string.IsNullOrEmpty(input));

                return input!;
            }

            if (volunteer != null)
            {
                Volunteer newVolunteer = new()
                {
                    ID = AskUserIfUpdate("Volunteer ID") && int.TryParse(GetValidInput("Enter volunteer ID: "), out int parsedID) ? parsedID : volunteer.ID,
                    Name = AskUserIfUpdate("Volunteer name") ? GetValidInput("Enter Volunteer name: ") : volunteer.Name,
                    Phone = AskUserIfUpdate("Cellphone number") ? GetValidInput("Enter Cellphone number: ") : volunteer.Phone,
                    Email = AskUserIfUpdate("Email") ? GetValidInput("Enter Email: ") : volunteer.Email,
                    Address = AskUserIfUpdate("Full Address") ? GetValidInput("Enter Full Address: ") : volunteer.Address,
                    Latitude = AskUserIfUpdate("Latitude") && double.TryParse(GetValidInput("Enter Latitude: "), out double parsedLatitude) ? parsedLatitude : volunteer.Latitude,
                    Longitude = AskUserIfUpdate("Longitude") && double.TryParse(GetValidInput("Enter Longitude: "), out double parsedLongitude) ? parsedLongitude : volunteer.Longitude,
                    Role = AskUserIfUpdate("Volunteer Role") && Enum.TryParse(GetValidInput("Enter Volunteer Role (Manager/Volunteer): "), out DO.Role parsedRole) ? parsedRole : volunteer.Role,
                    IsActive = AskUserIfUpdate("Active") && bool.TryParse(GetValidInput("Enter Active (True/False): "), out bool parsedIsActive) ? parsedIsActive : volunteer.IsActive,
                    DistanceType = AskUserIfUpdate("Distance Type") && Enum.TryParse(GetValidInput("Enter Distance Type (aerial_distance, walking_distance, driving_distance): "), out DO.DistanceType parsedDistanceTypes) ? parsedDistanceTypes : volunteer.DistanceType,
                    MaxDistanceForCall = AskUserIfUpdate("Max Distance") && double.TryParse(GetValidInput("Enter Max Distance: "), out double parsedMaxDistance) ? parsedMaxDistance : volunteer.MaxDistanceForCall,
                    Password = AskUserIfUpdate("Password") ? GetValidInput("Enter Password: ") : volunteer.Password,
                };
                s_dalVolunteer?.Update(newVolunteer);
                Console.WriteLine("Volunteer updated successfully.");
                Console.WriteLine(newVolunteer!.ToString());
            }
            else
            {
                Console.WriteLine("Volunteer not found.");
            }
        }

        private static void DeleteVolunteer()
        {
            Console.WriteLine("Enter Volunteer ID to delete:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID. Please enter a valid number.");
                return;
            }

            s_dalVolunteer!.Delete(id);
            Console.WriteLine("Volunteer deleted successfully!");
        }

        private static void DeleteAllVolunteers()
        {
            s_dalVolunteer!.DeleteAll();
            Console.WriteLine("All volunteers deleted successfully!");
        }

        private static void ConfigurationMenu()
        {
            Console.Clear();
            Console.WriteLine("\nConfiguration Menu:");
            Console.WriteLine("0. Back to Main Menu");
            Console.WriteLine("1. Advance Clock by 1 minute");
            Console.WriteLine("2. Advance Clock by 1 hour");
            Console.WriteLine("3. Advance Clock by 1 month");
            Console.WriteLine("4. Advance Clock by 1 year");
            Console.WriteLine("5. Display current Clock value");
            Console.WriteLine("6. Set a new value for any configuration variable");
            Console.WriteLine("7. Show current value for any configuration variable");
            Console.WriteLine("8. Reset Configuration");

            bool isRunning = true;

            do
            {
                try
                {
                    Console.Write("Enter your choice: ");
                    int choice = TryGetValidChoice();
                    switch (choice)
                    {
                        case 0: isRunning = false; break;
                        case 1: s_dalConfig!.Clock = s_dalConfig.Clock.AddMinutes(1); Console.WriteLine("Clock advanced by 1 minute."); break;
                        case 2: s_dalConfig!.Clock = s_dalConfig.Clock.AddHours(1); Console.WriteLine("Clock advanced by 1 hour."); break;
                        case 3: s_dalConfig!.Clock = s_dalConfig.Clock.AddMonths(1); Console.WriteLine("Clock advanced by 1 month."); break;
                        case 4: s_dalConfig!.Clock = s_dalConfig.Clock.AddYears(1); Console.WriteLine("Clock advanced by 1 year."); break;
                        case 5: Console.WriteLine($"Current Clock: {s_dalConfig!.Clock}"); break;
                        case 6: SetClockOrRiskRange(); ; break;
                        case 7: ShowClockOrRiskRange(); break;
                        case 8: s_dalConfig!.Reset(); Console.WriteLine("Configuration reset successfully!"); break;
                        default: Console.WriteLine("Invalid choice. Please try again."); break;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            } while (isRunning);
        }
        private static void SetClockOrRiskRange()
        {
            Console.WriteLine("Enter C/R for to choose Clock or RiskRange: ");
            if (!char.TryParse(Console.ReadLine(), out char choice))
                throw new FormatException("Invalid input. Please enter a single character.");
            switch (choice)
            {
                case 'C':
                    Console.WriteLine("Enter new Date: ");
                    if (!DateTime.TryParse(Console.ReadLine(), out DateTime newClock))
                    {
                        throw new FormatException("Invalid input. Please enter a single character.");
                    }
                    s_dalConfig!.Clock = newClock;
                    break;
                case 'R':
                    Console.WriteLine("Enter new Time Span: ");
                    if (!TimeSpan.TryParse(Console.ReadLine(), out TimeSpan newRiskRange))
                    {
                        throw new FormatException("Invalid input. Please enter a single character.");
                    }
                    s_dalConfig!.RiskRange = newRiskRange;
                    break;
                default:
                    Console.WriteLine("Wrong choice!");
                    break;
            }
        }
        private static void ShowClockOrRiskRange()
        {
            Console.WriteLine("Enter C/R for to choose Clock or RiskRange: ");
            if (!char.TryParse(Console.ReadLine(), out char choice))
                throw new FormatException("Invalid input. Please enter a single character.");
            switch (choice)
            {
                case 'C':
                    Console.WriteLine(s_dalConfig!.Clock);
                    break;
                case 'R':
                    Console.WriteLine(s_dalConfig!.RiskRange);
                    break;
                default:
                    Console.WriteLine("Wrong choice!");
                    break;
            }
        }

    }
}
