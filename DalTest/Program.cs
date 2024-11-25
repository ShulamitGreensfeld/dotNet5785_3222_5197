//using Dal;
//using DalApi;
//using DO;
////using System;
//using System.Security.Cryptography;

//namespace DalTest
//{
//    public static class Program
//    {
//        private static IAssignment? s_dalAssignment = new AssignmentImplementation();
//        private static ICall? s_dalCall = new CallImplementation();
//        private static IVolunteer? s_dalVolunteer = new VolunteerImplementation();
//        private static IConfig? s_dalConfig = new ConfigImplementation();

//        public static void Main()
//        {
//            try
//            {
//                while (true)
//                {
//                    PrintMainMenu();
//                    string? choice = Console.ReadLine()?.Trim();
//                    if (string.IsNullOrEmpty(choice) || choice.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;


//                    switch (choice)
//                    {
//                        case "1": ManageAssignments(); break;
//                        case "2": ManageCalls(); break;
//                        case "3": ManageVolunteers(); break;
//                        case "4": ManageConfiguration(); break;
//                        case "5": InitializeDatabase(); break;
//                        case "6": DisplayAllData(); break;
//                        case "7": ResetDatabase(); break;
//                        default: Console.WriteLine("Invalid choice. Please try again."); break;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"An error occurred: {ex.Message}");
//            }
//        }
//        private static void PrintMainMenu()
//        {
//            Console.WriteLine("\nMain Menu:");
//            Console.WriteLine("1. Manage Assignments");
//            Console.WriteLine("2. Manage Calls");
//            Console.WriteLine("3. Manage Volunteers");
//            Console.WriteLine("4. Manage Configuration");
//            Console.WriteLine("5. Initialize Database");
//            Console.WriteLine("6. Display All Data");
//            Console.WriteLine("7. Reset Database");
//            Console.WriteLine("Press 'exit' to quit.");
//            Console.Write("Enter your choice: ");
//        }

//        private static void ResetDatabase()
//        {
//            Console.WriteLine("Are you sure you want to reset the database? Type 'yes' to confirm.");
//            string? confirmation = Console.ReadLine();
//            if (confirmation?.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase) == true)
//            {
//                try
//                {
//                    s_dalAssignment!.DeleteAll();
//                    s_dalCall!.DeleteAll();
//                    s_dalVolunteer!.DeleteAll();
//                    s_dalConfig!.Reset();
//                    Console.WriteLine("Database and configuration reset successfully!");
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error resetting database: {ex.Message}");
//                }
//            }
//            else
//            {
//                Console.WriteLine("Database reset canceled.");
//            }
//        }




//        private static void ManageAssignments()
//        {
//            while (true)
//            {
//                Console.WriteLine("\nAssignment Menu:");
//                Console.WriteLine("1. Create Assignment");
//                Console.WriteLine("2. Read Assignment by ID");
//                Console.WriteLine("3. Read All Assignments");
//                Console.WriteLine("4. Update Assignment");
//                Console.WriteLine("5. Delete Assignment by ID");
//                Console.WriteLine("6. Delete All Assignments");
//                Console.WriteLine("Press 'exit' to return to the main menu.");
//                Console.Write("Enter your choice: ");

//                string? choice = Console.ReadLine();

//                if (choice == null || choice.Equals("exit", StringComparison.OrdinalIgnoreCase))
//                    break;

//                try
//                {
//                    switch (choice)
//                    {
//                        case "1":
//                            CreateAssignment();
//                            break;

//                        case "2":
//                            ReadAssignment();
//                            break;

//                        case "3":
//                            ReadAllAssignments();
//                            break;

//                        case "4":
//                            UpdateAssignment();
//                            break;

//                        case "5":
//                            DeleteAssignment();
//                            break;

//                        case "6":
//                            DeleteAllAssignments();
//                            break;

//                        default:
//                            Console.WriteLine("Invalid choice. Please try again.");
//                            break;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error: {ex.Message}");
//                }
//            }
//        }

//        private static void CreateAssignment()
//        {
//            Console.WriteLine("Enter Call ID:");
//            if (!int.TryParse(Console.ReadLine(), out int callId))
//            {
//                Console.WriteLine("Invalid Call ID. Please enter a valid number.");
//                return;
//            }

//            Console.WriteLine("Enter Volunteer ID:");
//            if (!int.TryParse(Console.ReadLine(), out int volunteerId))
//            {
//                Console.WriteLine("Invalid Volunteer ID. Please enter a valid number.");
//                return;
//            }

//            var assignment = new Assignment
//            {
//                CallId = callId,
//                VolunteerId = volunteerId,
//                EntryTimeForTreatment = s_dalConfig!.Clock // Using system clock
//            };
//            s_dalAssignment!.Create(assignment);
//            Console.WriteLine("Assignment created successfully!");
//        }

//        private static void ReadAssignment()
//        {
//            Console.WriteLine("Enter Assignment ID:");
//            if (!int.TryParse(Console.ReadLine(), out int id))
//            {
//                Console.WriteLine("Invalid Assignment ID. Please enter a valid number.");
//                return;
//            }

//            var readAssignment = s_dalAssignment!.Read(id);
//            Console.WriteLine(readAssignment != null ? readAssignment.ToString() : "Assignment not found.");
//        }

//        private static void ReadAllAssignments()
//        {
//            foreach (var item in s_dalAssignment!.ReadAll())
//                Console.WriteLine(item);
//        }

//        private static void UpdateAssignment()
//        {
//            Console.WriteLine("Enter Assignment ID to update:");
//            if (!int.TryParse(Console.ReadLine(), out int id))
//            {
//                Console.WriteLine("Invalid ID. Please enter a valid number.");
//                return;
//            }

//            var existingAssignment = s_dalAssignment!.Read(id);
//            if (existingAssignment != null)
//            {
//                Console.WriteLine($"Current Volunteer ID: {existingAssignment.VolunteerId}");
//                Console.WriteLine("Enter new Volunteer ID (leave empty to keep current):");
//                string? newVolunteerId = Console.ReadLine();
//                if (!string.IsNullOrWhiteSpace(newVolunteerId) && int.TryParse(newVolunteerId, out int newId))
//                    existingAssignment = existingAssignment with { VolunteerId = newId };
//                s_dalAssignment.Update(existingAssignment);
//                Console.WriteLine("Assignment updated successfully!");
//            }
//            else
//            {
//                Console.WriteLine("Assignment not found.");
//            }
//        }

//        private static void DeleteAssignment()
//        {
//            Console.WriteLine("Enter Assignment ID to delete:");
//            if (!int.TryParse(Console.ReadLine(), out int id))
//            {
//                Console.WriteLine("Invalid ID. Please enter a valid number.");
//                return;
//            }

//            s_dalAssignment!.Delete(id);
//            Console.WriteLine("Assignment deleted successfully!");
//        }

//        private static void DeleteAllAssignments()
//        {
//            s_dalAssignment!.DeleteAll();
//            Console.WriteLine("All assignments deleted successfully!");
//        }

//        private static void ManageCalls()
//        {
//            while (true)
//            {
//                Console.WriteLine("\nCall Menu:");
//                Console.WriteLine("1. Create Call");
//                Console.WriteLine("2. Read Call by ID");
//                Console.WriteLine("3. Read All Calls");
//                Console.WriteLine("4. Update Call");
//                Console.WriteLine("5. Delete Call by ID");
//                Console.WriteLine("6. Delete All Calls");
//                Console.WriteLine("Press 'exit' to return to main menu.");
//                Console.Write("Enter your choice: ");

//                string? choice = Console.ReadLine();

//                if (choice == null || choice.Equals("exit", StringComparison.OrdinalIgnoreCase))
//                    break;

//                try
//                {
//                    switch (choice)
//                    {
//                        case "1":
//                            CreateCall();
//                            break;

//                        case "2":
//                            ReadCall();
//                            break;

//                        case "3":
//                            ReadAllCalls();
//                            break;

//                        case "4":
//                            UpdateCall();
//                            break;

//                        case "5":
//                            DeleteCall();
//                            break;

//                        case "6":
//                            DeleteAllCalls();
//                            break;

//                        default:
//                            Console.WriteLine("Invalid choice. Please try again.");
//                            break;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error: {ex.Message}");
//                }
//            }
//        }

//        private static void CreateCall()
//        {
//            //Console.WriteLine("Enter Call Address:");
//            //string address = Console.ReadLine()!;
//            //var call = new Call
//            //{
//            //    Address = address,
//            //    OpeningTime = s_dalConfig!.Clock, // Using system clock
//            //    ClosingTime = s_dalConfig!.Clock.AddHours(1)
//            //};
//            //s_dalCall!.Create(call);
//            //Console.WriteLine("Call created successfully!");


//            Console.Clear();

//            // קריאה לסוג ה-call עם בדיקת תקינות
//            Console.Write("Enter Call Type (To Prepare Food, To Carry Food, To Package Food, To Donate Raw Materials, To Community Cooking Nights): ");
//            if (!Enum.TryParse(Console.ReadLine()!, out DO.TypeOfCall call_type))
//            {
//                throw new FormatException("Call Type is invalid!");
//            }

//            // קריאה לתיאור ה-call
//            Console.Write("Enter Call Description: ");
//            string verbal_description = Console.ReadLine()!;

//            // קריאה לכתובת ה-call
//            Console.Write("Enter Full Address: ");
//            string full_address = Console.ReadLine()!;

//            // קריאה לקואורדינטות (Latitude ו-Longitude) עם TryParse
//            Console.Write("Enter Latitude: ");
//            if (!double.TryParse(Console.ReadLine(), out double latitude))
//            {
//                throw new FormatException("Latitude is invalid!");
//            }

//            Console.Write("Enter Longitude: ");
//            if (!double.TryParse(Console.ReadLine(), out double longitude))
//            {
//                throw new FormatException("Longitude is invalid!");
//            }

//            // יצירת ה-call החדש
//            Call newCall = new Call
//            {
//                TypeOfCall = call_type,                      // המרה תקינה של Enum
//                CallDescription = verbal_description,    // שמירת תיאור
//                Address = full_address,                // שמירת כתובת
//                Latitude = latitude,                        // שמירת Latitude
//                Longitude = longitude,                      // שמירת Longitude
//            };

//            // יצירת ה-call ב-DAL
//            s_dalCall?.Create(newCall);
//            Console.WriteLine("Call created successfully.");
//        }

//        private static void ReadCall()
//        {
//            Console.Clear();
//            Console.Write("Enter Call ID to read: ");
//            int id = int.Parse(Console.ReadLine()!);
//            var call = s_dalCall?.Read(id);
//            if (call != null)
//            {
//                Console.WriteLine($"Call ID: {call.ID}, Call Type: {call.TypeOfCall}, " +
//                    $"Description: {call.CallDescription ?? "N/A"}, Full Address: {call.Address}, " +
//                    $"Latitude: {call.Latitude}, Longitude: {call.Longitude}, " +
//                    $"Opening Time: {call.OpeningTime}, Max Finish Time: {call.MaxTimeForClosing}");
//            }
//            else
//            {
//                Console.WriteLine("Call not found.");
//            }
//        }

//        private static void ReadAllCalls()
//        {
//            Console.Clear();
//            var calls = s_dalCall?.ReadAll();
//            if (calls != null)
//            {
//                foreach (var call in calls)
//                {
//                    Console.WriteLine($"Call ID: {call.ID}, Call Type: {call.TypeOfCall}, " +
//                        $"Description: {call.CallDescription ?? "N/A"}, Full Address: {call.Address}, " +
//                        $"Latitude: {call.Latitude}, Longitude: {call.Longitude}, " +
//                        $"Opening Time: {call.OpeningTime}, Max Finish Time: {call.MaxTimeForClosing}");
//                }
//            }
//            else
//            {
//                Console.WriteLine("No calls found.");
//            }
//        }

//        private static void UpdateCall()
//        {
//            Console.WriteLine("Enter Call ID to update:");
//            if (!int.TryParse(Console.ReadLine(), out int id))
//            {
//                Console.WriteLine("Invalid ID. Please enter a valid number.");
//                return;
//            }

//            var existingCall = s_dalCall!.Read(id);
//            if (existingCall != null)
//            {
//                Console.WriteLine($"Current Address: {existingCall.Address}");
//                Console.WriteLine("Enter new Address (leave empty to keep current):");
//                string? newAddress = Console.ReadLine();
//                if (!string.IsNullOrWhiteSpace(newAddress))
//                    existingCall = existingCall with { Address = newAddress };
//                s_dalCall.Update(existingCall);
//                Console.WriteLine("Call updated successfully!");
//            }
//            else
//            {
//                Console.WriteLine("Call not found.");
//            }
//        }

//        private static void DeleteCall()
//        {
//            Console.WriteLine("Enter Call ID to delete:");
//            if (!int.TryParse(Console.ReadLine(), out int id))
//            {
//                Console.WriteLine("Invalid ID. Please enter a valid number.");
//                return;
//            }

//            s_dalCall!.Delete(id);
//            Console.WriteLine("Call deleted successfully!");
//        }

//        private static void DeleteAllCalls()
//        {
//            s_dalCall!.DeleteAll();
//            Console.WriteLine("All calls deleted successfully!");
//        }

//        private static void ManageVolunteers()
//        {
//            while (true)
//            {
//                Console.WriteLine("\nVolunteer Menu:");
//                Console.WriteLine("1. Create Volunteer");
//                Console.WriteLine("2. Create Volunteer with Password"); // אופציה חדשה
//                Console.WriteLine("3. Read Volunteer by ID");
//                Console.WriteLine("4. Read All Volunteers");
//                Console.WriteLine("5. Update Volunteer");
//                Console.WriteLine("6. Update Volunteer Password"); // אופציה חדשה
//                Console.WriteLine("7. Update Volunteer DistanceType");
//                Console.WriteLine("8. Add Custom Attribute to Volunteer"); // אופציה חדשה
//                Console.WriteLine("9. Display Volunteer Attributes");      // אופציה חדשה
//                Console.WriteLine("10. Delete Volunteer by ID");
//                Console.WriteLine("11. Delete All Volunteers");
//                Console.WriteLine("Press 'exit' to return to the main menu.");
//                Console.Write("Enter your choice: ");

//                string? choice = Console.ReadLine();

//                if (choice == null || choice.Equals("exit", StringComparison.OrdinalIgnoreCase))
//                    break;

//                try
//                {
//                    switch (choice)
//                    {
//                        case "1": CreateVolunteer(); break;
//                        case "2": CreateVolunteerWithPassword(); break; // אופציה חדשה
//                        case "3": ReadVolunteer(); break;
//                        case "4": ReadAllVolunteers(); break;
//                        case "5": UpdateVolunteer(); break;
//                        case "6": UpdateVolunteerPassword(); break; // אופציה חדשה
//                        case "7": UpdateVolunteerDistanceType(); break;
//                        case "8": AddCustomAttributeToVolunteer(); break; // אופציה חדשה
//                        case "9": DisplayVolunteerAttributes(); break;      // אופציה חדשה
//                        case "10": DeleteVolunteer(); break;
//                        case "11": DeleteAllVolunteers(); break;
//                        default: Console.WriteLine("Invalid choice. Please try again."); break;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error: {ex.Message}");
//                }
//            }
//        }

//        private static void InitializeDatabase()
//        {
//            try
//            {
//                Initialization.Do(s_dalCall, s_dalAssignment, s_dalVolunteer, s_dalConfig);
//                Console.WriteLine("Database initialized successfully!");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Initialization failed: {ex.Message}");
//            }
//        }

//        private static void AddCustomAttributeToVolunteer()
//        {
//            Console.WriteLine("Enter Volunteer ID:");
//            if (!int.TryParse(Console.ReadLine(), out int id))
//            {
//                Console.WriteLine("Invalid Volunteer ID.");
//                return;
//            }

//            var volunteer = s_dalVolunteer!.Read(id);
//            if (volunteer == null)
//            {
//                Console.WriteLine("Volunteer not found.");
//                return;
//            }

//            Console.WriteLine("Enter attribute name:");
//            string attributeName = Console.ReadLine()!;

//            Console.WriteLine("Enter attribute value:");
//            string attributeValue = Console.ReadLine()!;

//            if (volunteer.CustomAttributes == null)
//                volunteer = volunteer with { CustomAttributes = new Dictionary<string, object>() };

//            volunteer.CustomAttributes[attributeName] = attributeValue;
//            s_dalVolunteer.Update(volunteer);

//            Console.WriteLine("Custom attribute added successfully!");
//        }
//        private static void DisplayVolunteerAttributes()
//        {
//            Console.WriteLine("Enter Volunteer ID:");
//            if (!int.TryParse(Console.ReadLine(), out int id))
//            {
//                Console.WriteLine("Invalid Volunteer ID.");
//                return;
//            }

//            var volunteer = s_dalVolunteer!.Read(id);
//            if (volunteer == null)
//            {
//                Console.WriteLine("Volunteer not found.");
//                return;
//            }

//            Console.WriteLine($"Volunteer ID: {volunteer.ID}");
//            Console.WriteLine($"Name: {volunteer.Name}");

//            if (volunteer.CustomAttributes != null && volunteer.CustomAttributes.Any())
//            {
//                Console.WriteLine("Custom Attributes:");
//                foreach (var attribute in volunteer.CustomAttributes)
//                {
//                    Console.WriteLine($"- {attribute.Key}: {attribute.Value}");
//                }
//            }
//            else
//            {
//                Console.WriteLine("No custom attributes found.");
//            }
//        }

//        private static void UpdateVolunteerDistanceType()
//        {
//            Console.WriteLine("Enter Volunteer ID:");
//            if (!int.TryParse(Console.ReadLine(), out int id))
//            {
//                Console.WriteLine("Invalid Volunteer ID. Please enter a valid number.");
//                return;
//            }

//            var volunteer = s_dalVolunteer!.Read(id);
//            if (volunteer != null)
//            {
//                Console.WriteLine($"Current DistanceType: {volunteer.DistanceType}");
//                Console.WriteLine("Choose new DistanceType (1-AirDistance, 2-WalkingDistance, 3-DrivingDistance): ");
//                if (int.TryParse(Console.ReadLine(), out int choice) &&
//                    Enum.IsDefined(typeof(DistanceType), choice - 1))
//                {
//                    volunteer = volunteer with { DistanceType = (DistanceType)(choice - 1) };
//                    s_dalVolunteer.Update(volunteer);
//                    Console.WriteLine("DistanceType updated successfully!");
//                }
//                else
//                {
//                    Console.WriteLine("Invalid DistanceType choice.");
//                }
//            }
//            else
//            {
//                Console.WriteLine("Volunteer not found.");
//            }
//        }

//        // בדיקת חוזק סיסמה
//        private static bool IsStrongPassword(string password)
//        {
//            if (password.Length < 8) return false;
//            if (!password.Any(char.IsUpper)) return false;
//            if (!password.Any(char.IsLower)) return false;
//            if (!password.Any(char.IsDigit)) return false;
//            if (!password.Any(ch => "!@#$%^&*()_+-=[]{}|;':\",.<>?/".Contains(ch))) return false;

//            return true;
//        }

//        // פונקציה להצפנת סיסמה (Hash)
//        private static string HashPassword(string password)
//        {
//            using (SHA256 sha256 = SHA256.Create())
//            {
//                byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
//                return Convert.ToBase64String(hashedBytes);
//            }
//        }

//        // פונקציה לאימות סיסמה
//        private static bool VerifyPassword(string enteredPassword, string storedHash)
//        {
//            string hashOfEntered = HashPassword(enteredPassword);
//            return hashOfEntered == storedHash;
//        }

//        private static void CreateVolunteerWithPassword()
//        {
//            Console.WriteLine("Enter Volunteer Name:");
//            string name = Console.ReadLine()!;

//            Console.WriteLine("Enter Volunteer Email:");
//            string email = Console.ReadLine()!;

//            Console.WriteLine("Enter Volunteer Password:");
//            string password = Console.ReadLine()!;

//            if (!IsStrongPassword(password))
//            {
//                Console.WriteLine("Password is not strong enough. Try again.");
//                return;
//            }

//            string passwordHash = HashPassword(password);

//            var volunteer = new Volunteer
//            {
//                ID = new Random().Next(100000, 999999), // מספר אקראי כ-ID
//                Name = name,
//                Email = email,
//                PasswordHash = passwordHash
//            };

//            s_dalVolunteer!.Create(volunteer);
//            Console.WriteLine("Volunteer created successfully with an encrypted password.");
//        }

//        private static void UpdateVolunteerPassword()
//        {
//            Console.WriteLine("Enter Volunteer ID:");
//            if (!int.TryParse(Console.ReadLine(), out int id))
//            {
//                Console.WriteLine("Invalid ID.");
//                return;
//            }

//            var volunteer = s_dalVolunteer!.Read(id);
//            if (volunteer == null)
//            {
//                Console.WriteLine("Volunteer not found.");
//                return;
//            }

//            Console.WriteLine("Enter current password:");
//            string currentPassword = Console.ReadLine()!;

//            if (!VerifyPassword(currentPassword, volunteer.PasswordHash!))
//            {
//                Console.WriteLine("Incorrect current password.");
//                return;
//            }

//            Console.WriteLine("Enter new password:");
//            string newPassword = Console.ReadLine()!;

//            if (!IsStrongPassword(newPassword))
//            {
//                Console.WriteLine("Password is not strong enough. Try again.");
//                return;
//            }

//            string newPasswordHash = HashPassword(newPassword);
//            volunteer = volunteer with { PasswordHash = newPasswordHash };
//            s_dalVolunteer.Update(volunteer);

//            Console.WriteLine("Password updated successfully.");
//        }

//        private static void CreateVolunteer()
//        {
//            Console.WriteLine("Enter Volunteer Name:");
//            string name = Console.ReadLine()!;
//            var volunteer = new Volunteer
//            {
//                Name = name
//            };
//            s_dalVolunteer!.Create(volunteer);
//            Console.WriteLine("Volunteer created successfully!");
//        }

//        private static void ReadVolunteer()
//        {
//            Console.WriteLine("Enter Volunteer ID:");
//            if (!int.TryParse(Console.ReadLine(), out int id))
//            {
//                Console.WriteLine("Invalid Volunteer ID. Please enter a valid number.");
//                return;
//            }

//            var readVolunteer = s_dalVolunteer!.Read(id);
//            Console.WriteLine(readVolunteer != null ? readVolunteer.ToString() : "Volunteer not found.");
//        }

//        private static void ReadAllVolunteers()
//        {
//            foreach (var item in s_dalVolunteer!.ReadAll())
//                Console.WriteLine(item);
//        }

//        private static void UpdateVolunteer()
//        {
//            Console.WriteLine("Enter Volunteer ID to update:");
//            if (!int.TryParse(Console.ReadLine(), out int id))
//            {
//                Console.WriteLine("Invalid ID. Please enter a valid number.");
//                return;
//            }

//            var existingVolunteer = s_dalVolunteer!.Read(id);
//            if (existingVolunteer != null)
//            {
//                Console.WriteLine($"Current Name: {existingVolunteer.Name}");
//                Console.WriteLine("Enter new Name (leave empty to keep current):");
//                string? newName = Console.ReadLine();
//                if (!string.IsNullOrWhiteSpace(newName))
//                    existingVolunteer = existingVolunteer with { Name = newName };
//                s_dalVolunteer.Update(existingVolunteer);
//                Console.WriteLine("Volunteer updated successfully!");
//            }
//            else
//            {
//                Console.WriteLine("Volunteer not found.");
//            }
//        }

//        private static void DeleteVolunteer()
//        {
//            Console.WriteLine("Enter Volunteer ID to delete:");
//            if (!int.TryParse(Console.ReadLine(), out int id))
//            {
//                Console.WriteLine("Invalid ID. Please enter a valid number.");
//                return;
//            }

//            s_dalVolunteer!.Delete(id);
//            Console.WriteLine("Volunteer deleted successfully!");
//        }

//        private static void DeleteAllVolunteers()
//        {
//            s_dalVolunteer!.DeleteAll();
//            Console.WriteLine("All volunteers deleted successfully!");
//        }

//        private static void ManageConfiguration()
//        {
//            while (true)
//            {
//                Console.WriteLine("\nConfiguration Menu:");
//                Console.WriteLine("1. Advance Clock by 1 minute");
//                Console.WriteLine("2. Advance Clock by 1 hour");
//                Console.WriteLine("3. Display current Clock value");
//                Console.WriteLine("4. Reset Configuration");
//                Console.WriteLine("Press 'exit' to return to main menu.");
//                Console.Write("Enter your choice: ");

//                string? choice = Console.ReadLine();

//                if (choice == null || choice.Equals("exit", StringComparison.OrdinalIgnoreCase))
//                    break;

//                try
//                {
//                    switch (choice)
//                    {
//                        case "1":
//                            s_dalConfig!.Clock = s_dalConfig.Clock.AddMinutes(1);
//                            Console.WriteLine("Clock advanced by 1 minute.");
//                            break;
//                        case "2":
//                            s_dalConfig!.Clock = s_dalConfig.Clock.AddHours(1);
//                            Console.WriteLine("Clock advanced by 1 hour.");
//                            break;
//                        case "3":
//                            Console.WriteLine($"Current Clock: {s_dalConfig!.Clock}");
//                            break;
//                        case "4":
//                            s_dalConfig!.Reset();
//                            Console.WriteLine("Configuration reset successfully!");
//                            break;
//                        default:
//                            Console.WriteLine("Invalid choice. Please try again.");
//                            break;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error: {ex.Message}");
//                }
//            }
//        }
//        private static void DisplayAllData()
//        {
//            try
//            {
//                Console.WriteLine("\nAll Assignments:");
//                foreach (var assignment in s_dalAssignment!.ReadAll())
//                    Console.WriteLine(assignment);

//                Console.WriteLine("\nAll Calls:");
//                foreach (var call in s_dalCall!.ReadAll())
//                    Console.WriteLine(call);

//                Console.WriteLine("\nAll Volunteers:");
//                foreach (var volunteer in s_dalVolunteer!.ReadAll())
//                    Console.WriteLine(volunteer);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error displaying data: {ex.Message}");
//            }
//        }

//    }
//}



using Dal;
using DalApi;
using DO;
using System.Security.Cryptography;

namespace DalTest
{
    public static class Program
    {
        private static IAssignment? s_dalAssignment = new AssignmentImplementation();
        private static ICall? s_dalCall = new CallImplementation();
        private static IVolunteer? s_dalVolunteer = new VolunteerImplementation();
        private static IConfig? s_dalConfig = new ConfigImplementation();

        public static void Main()
        {
            try
            {
                while (true)
                {
                    PrintMainMenu();
                    string? choice = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(choice) || choice.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;


                    switch (choice)
                    {
                        case "1": ManageAssignments(); break;
                        case "2": ManageCalls(); break;
                        case "3": ManageVolunteers(); break;
                        case "4": ManageConfiguration(); break;
                        case "5": InitializeDatabase(); break;
                        case "6": DisplayAllData(); break;
                        case "7": ResetDatabase(); break;
                        default: Console.WriteLine("Invalid choice. Please try again."); break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        private static void PrintMainMenu()
        {
            Console.WriteLine("\nMain Menu:");
            Console.WriteLine("1. Assignments Menu");
            Console.WriteLine("2. Calls Menu");
            Console.WriteLine("3. Volunteers Menu");
            Console.WriteLine("4. Configuration Menu");
            Console.WriteLine("5. Initialize Database");
            Console.WriteLine("6. Display All Data");
            Console.WriteLine("7. Reset Database");
            Console.WriteLine("Press 'exit' to quit.");
            Console.Write("Enter your choice: ");
        }

        private static void ResetDatabase()
        {
            Console.WriteLine("Are you sure you want to reset the database? Press 'yes' to confirm.");
            string? confirmation = Console.ReadLine();
            if (confirmation?.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase) == true)
            {
                try
                {
                    s_dalAssignment!.DeleteAll();
                    s_dalCall!.DeleteAll();
                    s_dalVolunteer!.DeleteAll();
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




        private static void ManageAssignments()
        {
            while (true)
            {
                Console.WriteLine("\nAssignment Menu:");
                Console.WriteLine("1. Create Assignment");
                Console.WriteLine("2. Read Assignment by ID");
                Console.WriteLine("3. Read All Assignments");
                Console.WriteLine("4. Update Assignment");
                Console.WriteLine("5. Delete Assignment by ID");
                Console.WriteLine("6. Delete All Assignments");
                Console.WriteLine("Press 'exit' to return to the main menu.");
                Console.Write("Enter your choice: ");

                string? choice = Console.ReadLine();

                if (choice == null || choice.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                try
                {
                    switch (choice)
                    {
                        case "1":
                            CreateAssignment();
                            break;

                        case "2":
                            ReadAssignment();
                            break;

                        case "3":
                            ReadAllAssignments();
                            break;

                        case "4":
                            UpdateAssignment();
                            break;

                        case "5":
                            DeleteAssignment();
                            break;

                        case "6":
                            DeleteAllAssignments();
                            break;

                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private static void CreateAssignment()
        {
            Console.WriteLine("Enter Call ID:");
            if (!int.TryParse(Console.ReadLine(), out int callId))
            {
                Console.WriteLine("Invalid Call ID. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter Volunteer ID:");
            if (!int.TryParse(Console.ReadLine(), out int volunteerId))
            {
                Console.WriteLine("Invalid Volunteer ID. Please enter a valid number.");
                return;
            }

            var assignment = new Assignment
            {
                CallId = callId,
                VolunteerId = volunteerId,
                EntryTimeForTreatment = s_dalConfig!.Clock // Using system clock
            };
            s_dalAssignment!.Create(assignment);
            Console.WriteLine("Assignment created successfully!");
        }

        private static void ReadAssignment()
        {
            Console.WriteLine("Enter Assignment ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Assignment ID. Please enter a valid number.");
                return;
            }

            var readAssignment = s_dalAssignment!.Read(id);
            Console.WriteLine(readAssignment != null ? readAssignment.ToString() : "Assignment not found.");
        }

        private static void ReadAllAssignments()
        {
            foreach (var item in s_dalAssignment!.ReadAll())
                Console.WriteLine(item);
        }

        private static void UpdateAssignment()
        {
            Console.WriteLine("Enter Assignment ID to update:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID. Please enter a valid number.");
                return;
            }

            var existingAssignment = s_dalAssignment!.Read(id);
            if (existingAssignment != null)
            {
                Console.WriteLine($"Current Volunteer ID: {existingAssignment.VolunteerId}");
                Console.WriteLine("Enter new Volunteer ID (leave empty to keep current):");
                string? newVolunteerId = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newVolunteerId) && int.TryParse(newVolunteerId, out int newId))
                    existingAssignment = existingAssignment with { VolunteerId = newId };
                s_dalAssignment.Update(existingAssignment);
                Console.WriteLine("Assignment updated successfully!");
            }
            else
            {
                Console.WriteLine("Assignment not found.");
            }
        }

        private static void DeleteAssignment()
        {
            Console.WriteLine("Enter Assignment ID to delete:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID. Please enter a valid number.");
                return;
            }

            s_dalAssignment!.Delete(id);
            Console.WriteLine("Assignment deleted successfully!");
        }

        private static void DeleteAllAssignments()
        {
            s_dalAssignment!.DeleteAll();
            Console.WriteLine("All assignments deleted successfully!");
        }

        private static void ManageCalls()
        {
            while (true)
            {
                Console.WriteLine("\nCall Menu:");
                Console.WriteLine("1. Create Call");
                Console.WriteLine("2. Read Call by ID");
                Console.WriteLine("3. Read All Calls");
                Console.WriteLine("4. Update Call");
                Console.WriteLine("5. Delete Call by ID");
                Console.WriteLine("6. Delete All Calls");
                Console.WriteLine("Press 'exit' to return to the main menu.");
                Console.Write("Enter your choice: ");

                string? choice = Console.ReadLine();

                if (choice == null || choice.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                try
                {
                    switch (choice)
                    {
                        case "1":
                            CreateCall();
                            break;

                        case "2":
                            ReadCall();
                            break;

                        case "3":
                            ReadAllCalls();
                            break;

                        case "4":
                            UpdateCall();
                            break;

                        case "5":
                            DeleteCall();
                            break;

                        case "6":
                            DeleteAllCalls();
                            break;

                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private static void CreateCall()
        {
            Console.WriteLine("Enter Call Address:");
            string address = Console.ReadLine()!;
            var call = new Call
            {
                Address = address,
                OpeningTime = s_dalConfig!.Clock, // Using system clock
                ClosingTime = s_dalConfig!.Clock.AddHours(1)
            };
            s_dalCall!.Create(call);
            Console.WriteLine("Call created successfully!");
        }

        private static void ReadCall()
        {
            Console.WriteLine("Enter Call ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Call ID. Please enter a valid number.");
                return;
            }

            var readCall = s_dalCall!.Read(id);
            Console.WriteLine(readCall != null ? readCall.ToString() : "Call not found.");
        }

        private static void ReadAllCalls()
        {
            foreach (var item in s_dalCall!.ReadAll())
                Console.WriteLine(item);
        }

        private static void UpdateCall()
        {
            Console.WriteLine("Enter Call ID to update:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID. Please enter a valid number.");
                return;
            }

            var existingCall = s_dalCall!.Read(id);
            if (existingCall != null)
            {
                Console.WriteLine($"Current Address: {existingCall.Address}");
                Console.WriteLine("Enter new Address (leave empty to keep current):");
                string? newAddress = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newAddress))
                    existingCall = existingCall with { Address = newAddress };
                s_dalCall.Update(existingCall);
                Console.WriteLine("Call updated successfully!");
            }
            else
            {
                Console.WriteLine("Call not found.");
            }
        }

        private static void DeleteCall()
        {
            Console.WriteLine("Enter Call ID to delete:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID. Please enter a valid number.");
                return;
            }

            s_dalCall!.Delete(id);
            Console.WriteLine("Call deleted successfully!");
        }

        private static void DeleteAllCalls()
        {
            s_dalCall!.DeleteAll();
            Console.WriteLine("All calls deleted successfully!");
        }

        private static void ManageVolunteers()
        {
            while (true)
            {
                Console.WriteLine("\nVolunteer Menu:");
                Console.WriteLine("1. Create Volunteer");
                Console.WriteLine("2. Create Volunteer with Password"); // אופציה חדשה
                Console.WriteLine("3. Read Volunteer by ID");
                Console.WriteLine("4. Read All Volunteers");
                Console.WriteLine("5. Update Volunteer");
                Console.WriteLine("6. Update Volunteer Password"); // אופציה חדשה
                Console.WriteLine("7. Update Volunteer DistanceType");
                Console.WriteLine("8. Add Custom Attribute to Volunteer"); // אופציה חדשה
                Console.WriteLine("9. Display Volunteer Attributes");      // אופציה חדשה
                Console.WriteLine("10. Delete Volunteer by ID");
                Console.WriteLine("11. Delete All Volunteers");
                Console.WriteLine("Press 'exit' to return to the main menu.");
                Console.Write("Enter your choice: ");

                string? choice = Console.ReadLine();

                if (choice == null || choice.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                try
                {
                    switch (choice)
                    {
                        case "1": CreateVolunteer(); break;
                        case "2": CreateVolunteerWithPassword(); break; // אופציה חדשה
                        case "3": ReadVolunteer(); break;
                        case "4": ReadAllVolunteers(); break;
                        case "5": UpdateVolunteer(); break;
                        case "6": UpdateVolunteerPassword(); break; // אופציה חדשה
                        case "7": UpdateVolunteerDistanceType(); break;
                        case "8": AddCustomAttributeToVolunteer(); break; // אופציה חדשה
                        case "9": DisplayVolunteerAttributes(); break;      // אופציה חדשה
                        case "10": DeleteVolunteer(); break;
                        case "11": DeleteAllVolunteers(); break;
                        default: Console.WriteLine("Invalid choice. Please try again."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
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

        private static void AddCustomAttributeToVolunteer()
        {
            Console.WriteLine("Enter Volunteer ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Volunteer ID.");
                return;
            }

            var volunteer = s_dalVolunteer!.Read(id);
            if (volunteer == null)
            {
                Console.WriteLine("Volunteer not found.");
                return;
            }

            Console.WriteLine("Enter attribute name:");
            string attributeName = Console.ReadLine()!;

            Console.WriteLine("Enter attribute value:");
            string attributeValue = Console.ReadLine()!;

            if (volunteer.CustomAttributes == null)
                volunteer = volunteer with { CustomAttributes = new Dictionary<string, object>() };

            volunteer.CustomAttributes[attributeName] = attributeValue;
            s_dalVolunteer.Update(volunteer);

            Console.WriteLine("Custom attribute added successfully!");
        }
        private static void DisplayVolunteerAttributes()
        {
            Console.WriteLine("Enter Volunteer ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Volunteer ID.");
                return;
            }

            var volunteer = s_dalVolunteer!.Read(id);
            if (volunteer == null)
            {
                Console.WriteLine("Volunteer not found.");
                return;
            }

            Console.WriteLine($"Volunteer ID: {volunteer.ID}");
            Console.WriteLine($"Name: {volunteer.Name}");

            if (volunteer.CustomAttributes != null && volunteer.CustomAttributes.Any())
            {
                Console.WriteLine("Custom Attributes:");
                foreach (var attribute in volunteer.CustomAttributes)
                {
                    Console.WriteLine($"- {attribute.Key}: {attribute.Value}");
                }
            }
            else
            {
                Console.WriteLine("No custom attributes found.");
            }
        }

        private static void UpdateVolunteerDistanceType()
        {
            Console.WriteLine("Enter Volunteer ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Volunteer ID. Please enter a valid number.");
                return;
            }

            var volunteer = s_dalVolunteer!.Read(id);
            if (volunteer != null)
            {
                Console.WriteLine($"Current DistanceType: {volunteer.DistanceType}");
                Console.WriteLine("Choose new DistanceType (1-AirDistance, 2-WalkingDistance, 3-DrivingDistance): ");
                if (int.TryParse(Console.ReadLine(), out int choice) &&
                    Enum.IsDefined(typeof(DistanceType), choice - 1))
                {
                    volunteer = volunteer with { DistanceType = (DistanceType)(choice - 1) };
                    s_dalVolunteer.Update(volunteer);
                    Console.WriteLine("DistanceType updated successfully!");
                }
                else
                {
                    Console.WriteLine("Invalid DistanceType choice.");
                }
            }
            else
            {
                Console.WriteLine("Volunteer not found.");
            }
        }

        // בדיקת חוזק סיסמה
        private static bool IsStrongPassword(string password)
        {
            if (password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(ch => "!@#$%^&*()_+-=[]{}|;':\",.<>?/".Contains(ch))) return false;

            return true;
        }

        // פונקציה להצפנת סיסמה (Hash)
        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // פונקציה לאימות סיסמה
        private static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            string hashOfEntered = HashPassword(enteredPassword);
            return hashOfEntered == storedHash;
        }

        private static void CreateVolunteerWithPassword()
        {
            Console.WriteLine("Enter Volunteer Name:");
            string name = Console.ReadLine()!;

            Console.WriteLine("Enter Volunteer Email:");
            string email = Console.ReadLine()!;

            Console.WriteLine("Enter Volunteer Password:");
            string password = Console.ReadLine()!;

            if (!IsStrongPassword(password))
            {
                Console.WriteLine("Password is not strong enough. Try again.");
                return;
            }

            string passwordHash = HashPassword(password);

            var volunteer = new Volunteer
            {
                ID = new Random().Next(100000, 999999), // מספר אקראי כ-ID
                Name = name,
                Email = email,
                PasswordHash = passwordHash
            };

            s_dalVolunteer!.Create(volunteer);
            Console.WriteLine("Volunteer created successfully with an encrypted password.");
        }

        private static void UpdateVolunteerPassword()
        {
            Console.WriteLine("Enter Volunteer ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            var volunteer = s_dalVolunteer!.Read(id);
            if (volunteer == null)
            {
                Console.WriteLine("Volunteer not found.");
                return;
            }

            Console.WriteLine("Enter current password:");
            string currentPassword = Console.ReadLine()!;

            if (!VerifyPassword(currentPassword, volunteer.PasswordHash!))
            {
                Console.WriteLine("Incorrect current password.");
                return;
            }

            Console.WriteLine("Enter new password:");
            string newPassword = Console.ReadLine()!;

            if (!IsStrongPassword(newPassword))
            {
                Console.WriteLine("Password is not strong enough. Try again.");
                return;
            }

            string newPasswordHash = HashPassword(newPassword);
            volunteer = volunteer with { PasswordHash = newPasswordHash };
            s_dalVolunteer.Update(volunteer);

            Console.WriteLine("Password updated successfully.");
        }

        private static void CreateVolunteer()
        {
            Console.WriteLine("Enter Volunteer Name:");
            string name = Console.ReadLine()!;
            var volunteer = new Volunteer
            {
                Name = name
            };
            s_dalVolunteer!.Create(volunteer);
            Console.WriteLine("Volunteer created successfully!");
        }

        private static void ReadVolunteer()
        {
            Console.WriteLine("Enter Volunteer ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Volunteer ID. Please enter a valid number.");
                return;
            }

            var readVolunteer = s_dalVolunteer!.Read(id);
            Console.WriteLine(readVolunteer != null ? readVolunteer.ToString() : "Volunteer not found.");
        }

        private static void ReadAllVolunteers()
        {
            foreach (var item in s_dalVolunteer!.ReadAll())
                Console.WriteLine(item);
        }

        private static void UpdateVolunteer()
        {
            Console.WriteLine("Enter Volunteer ID to update:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID. Please enter a valid number.");
                return;
            }

            var existingVolunteer = s_dalVolunteer!.Read(id);
            if (existingVolunteer != null)
            {
                Console.WriteLine($"Current Name: {existingVolunteer.Name}");
                Console.WriteLine("Enter new Name (leave empty to keep current):");
                string? newName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newName))
                    existingVolunteer = existingVolunteer with { Name = newName };
                s_dalVolunteer.Update(existingVolunteer);
                Console.WriteLine("Volunteer updated successfully!");
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

        private static void ManageConfiguration()
        {
            while (true)
            {
                Console.WriteLine("\nConfiguration Menu:");
                Console.WriteLine("1. Advance Clock by 1 minute");
                Console.WriteLine("2. Advance Clock by 1 hour");
                Console.WriteLine("3. Display current Clock value");
                Console.WriteLine("4. Reset Configuration");
                Console.WriteLine("Press 'exit' to return to main menu.");
                Console.Write("Enter your choice: ");

                string? choice = Console.ReadLine();

                if (choice == null || choice.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                try
                {
                    switch (choice)
                    {
                        case "1":
                            s_dalConfig!.Clock = s_dalConfig.Clock.AddMinutes(1);
                            Console.WriteLine("Clock advanced by 1 minute.");
                            break;
                        case "2":
                            s_dalConfig!.Clock = s_dalConfig.Clock.AddHours(1);
                            Console.WriteLine("Clock advanced by 1 hour.");
                            break;
                        case "3":
                            Console.WriteLine($"Current Clock: {s_dalConfig!.Clock}");
                            break;
                        case "4":
                            s_dalConfig!.Reset();
                            Console.WriteLine("Configuration reset successfully!");
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
        private static void DisplayAllData()
        {
            try
            {
                Console.WriteLine("\nAll Assignments:");
                foreach (var assignment in s_dalAssignment!.ReadAll())
                    Console.WriteLine(assignment);

                Console.WriteLine("\nAll Calls:");
                foreach (var call in s_dalCall!.ReadAll())
                    Console.WriteLine(call);

                Console.WriteLine("\nAll Volunteers:");
                foreach (var volunteer in s_dalVolunteer!.ReadAll())
                    Console.WriteLine(volunteer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying data: {ex.Message}");
            }
        }

    }
}