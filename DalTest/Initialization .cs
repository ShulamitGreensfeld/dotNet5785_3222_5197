using DalApi;
using DO;
using Microsoft.VisualBasic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DalTest
{
    public static class Initialization
    {
        //private static ICall? s_dalCall; //stage 1
        //private static IVolunteer? s_dalVolunteer; //stage 1
        //private static IAssignment? s_dalAssignment; //stage 1
        //private static IConfig? s_dalConfig; //stage 1
        private static IDal? s_dal; //stage 2
        private static readonly Random s_rand = new();//stage 4


        /// <summary>
        /// Initializes the database with random data.
        /// </summary>

        //public static void Do(IStudent? dalStudent, ICourse? dalCourse, ILink? dalStudentInCourse, IConfig? dalConfig) // stage 1
        //public static void Do(IDal dal) //stage 2
        public static void Do() //stage 4
        {
            //s_dalCall = dalCall ?? throw new NullReferenceException("DAL for Calls cannot be null!"); //stage 1
            //s_dalAssignment = dalAssignment ?? throw new NullReferenceException("DAL for Assignments cannot be null!"); //stage 1
            //s_dalVolunteer = dalVolunteer ?? throw new NullReferenceException("DAL for Volunteers cannot be null!"); //stage 1
            //s_dalConfig = dalConfig ?? throw new NullReferenceException("DAL for Config cannot be null!"); //stage 1
            //s_dal = dal ?? throw new NullReferenceException("DAL object can not be null!"); //stage 2
            s_dal = DalApi.Factory.Get; //stage 4

            Console.WriteLine("Resetting configuration and clearing data...");
            //s_dalConfig.Reset(); //stage 1
            //s_dalCall.DeleteAll();//stage 1
            //s_dalVolunteer.DeleteAll(); //stage 1
            //s_dalAssignment.DeleteAll(); //stage 1
            s_dal.ResetDB();//stage 2
              createSuperAdmin();

            Console.WriteLine("Initializing Volunteers list ...");
            Console.WriteLine("Initializing Calls list ...");
            Console.WriteLine("Initializing Assignments list ...");
            createVolunteer();
            createCall();
            createAssignments();
            Console.WriteLine("Initialization completed successfully!");
        }

        /// <summary>
        /// Creates calls with realistic data.
        /// Generates a list of calls with descriptions, addresses, coordinates, and opening times,
        /// and inserts them into the database.
        /// </summary>
        private static void createCall()
        {
            DateTime systemTime = s_dal!.Config.Clock;
            DateTime callOpeningTime;
            DateTime? callMaxFinishTime;
            Random rand = new Random();
            CallType[] callsTypes = Enum.GetValues(typeof(CallType)).Cast<CallType>().ToArray();
            string[] CallDescriptions =
            {
                "Help us package meals for soldiers in the field—your support makes it happen!",
                "Use your cooking skills to make fresh meals for soldiers far from home.",
                "Volunteer to deliver meal packages to military bases—they’re counting on us!",
                "Join us for a fun community cooking night to prepare meals for soldiers.",
                "Support our efforts by donating fresh vegetables for meal prep.",
                "Join the team to organize and pack food for delivery.",
                "Bake cookies, cakes, or treats to lift a soldier’s spirits.",
                "Use your vehicle to transport food to soldiers in training areas.",
                "Spend an evening cooking together while making a difference.",
                "Contribute pasta, rice, or staples that are essential for cooking.",
                "Be part of wrapping sandwiches and snacks for those protecting us.",
                "Join us in preparing soups, stews, or casseroles—comfort food for heroes.",
                "Help us carry freshly prepared meals to emergency locations.",
                "Help prepare meals alongside neighbors and friends for a great cause.",
                "Donate sauces, spices, or oils to add flavor to meals for soldiers.",
                "Package hot meals into containers and bring a smile to someone’s day.",
                "Help us cook using donated ingredients to create something special.",
                "Make a difference by delivering food to remote outposts.",
                "Be part of a community effort to support those who protect us.",
                "Donate canned goods or non-perishable food items.",
                "Help prepare food items and pack them for easy transport.",
                "Work with a cooking team to prepare large quantities of meals.",
                "Drive and deliver meals, ensuring everything stays fresh and intact.",
                "Share your favorite recipes while cooking together for a meaningful goal.",
                "Share baking ingredients like sugar or flour to create homemade treats.",
                "Work alongside others to efficiently pack food for our soldiers.",
                "Chop vegetables and prep ingredients to kick-start our kitchen efforts.",
                "Transport bulk food supplies for distribution to multiple locations.",
                "Enjoy a team-building evening filled with cooking and laughter.",
                "Offer fresh fruits that can be turned into desserts or healthy snacks.",
                "Add a special touch by labeling meal packages with care.",
                "Add flavor to the day by helping season and cook delicious meals.",
                "Help distribute meal packages directly to the soldiers who need them most.",
                "Help us prepare large meals in a lively and welcoming environment.",
                "Donate raw meat or fish to ensure our meals are rich in protein.",
                "Seal and secure food containers to ensure everything stays fresh.",
                "Bake bread or pastries that will warm someone’s heart.",
                "Deliver care packages filled with love and nourishment.",
                "Participate in a special event to cook holiday meals for soldiers.",
                "Supply grains, legumes, or other bulk items for nutritious meals.",
                "Prepare boxes of pre-cooked meals ready for delivery.",
                "Create balanced, nutritious meals that give soldiers the energy they need.",
                "Join our team of volunteer drivers and bring the food where it’s needed.",
                "Bring your energy and cook side by side with other caring individuals.",
                "Provide dairy products like cheese, butter, or milk to enhance our cooking.",
                "Assist in packing large trays of delicious food for group delivery.",
                "Bring your passion for cooking and help us make hearty, filling dishes.",
                "Help us stock up on essential kitchen supplies like salt, pepper, and spices.",
                "Join a social cooking night and put your culinary skills to good use.",
                "Donate fresh herbs or seasonings to give meals a special homemade touch."
            };

            string[] CallAddresses = {
              "120 Dizengoff St, Tel Aviv",
              "15 Herzl St, Haifa",
              "30 Jaffa St, Jerusalem",
              "22 Rothschild Blvd, Tel Aviv",
              "10 Derech HaShalom, Ramat Gan",
              "1 HaNassi Blvd, Haifa",
              "45 HaAtzmaut Blvd, Ashdod",
              "88 Allenby St, Tel Aviv",
              "12 Ben Gurion Blvd, Bat Yam",
              "20 King George St, Jerusalem",
              "132 Begin Road, Tel Aviv",
              "8 Keren Hayesod St, Jerusalem",
              "99 Ben Yehuda St, Tel Aviv",
              "25 Weizmann St, Kfar Saba",
              "14 Golani Brigade St, Afula",
              "33 Yehuda Halevi St, Tel Aviv",
              "77 HaNassi Blvd, Haifa",
              "19 Arlozorov St, Tel Aviv",
              "96 HaHashmonaim St, Tel Aviv",
              "12 Menachem Begin Blvd, Rishon LeZion",
              "5 Bialik St, Ramat Gan",
              "50 HaYarkon St, Tel Aviv",
              "7 Hillel St, Jerusalem",
              "60 Nordau Blvd, Netanya",
              "3 Rambam St, Be'er Sheva",
              "18 HaPalmach St, Rehovot",
              "22 HaTmarim Blvd, Eilat",
              "9 HaGalil St, Tiberias",
              "11 HaShalom St, Petah Tikva",
              "27 HaEmek St, Karmiel",
              "34 HaNasi St, Herzliya",
              "6 HaAliya St, Rishon LeZion",
              "15 HaAtzmaut St, Ashkelon",
              "21 HaDekel St, Holon",
              "8 HaGefen St, Kfar Saba",
              "13 HaShikma St, Lod",
              "29 HaZayit St, Nahariya",
              "17 HaTe'ena St, Ra'anana",
              "4 HaNarkis St, Modi'in",
              "26 HaRimon St, Givatayim",
              "31 HaTavor St, Rosh HaAyin",
              "2 HaKalanit St, Nesher",
              "14 HaLotos St, Yavne",
              "23 HaNarkis St, Kiryat Ata",
              "37 HaSharon St, Hadera",
              "10 HaGolan St, Kiryat Shmona",
              "19 HaCarmel St, Safed",
              "25 HaBashan St, Ma'alot-Tarshiha",
              "12 HaHermon St, Migdal HaEmek",
              "40 Herzl St, Tel Aviv"
            };

            double[] CallLatitudes = {
          32.0736, 32.0739, 32.8194, 32.0702, 31.7702, 32.0782, 32.3212, 32.0951, 32.1575, 29.5596,
          32.1749, 32.0926, 32.0814, 32.1311, 31.7665, 32.0509, 32.0354, 31.9737, 32.0656, 31.6730,
          31.7717, 32.0546, 31.8419, 31.7134, 32.0779, 32.0944, 32.1736, 31.6534, 32.9885, 32.6047,
          32.9023, 31.6713, 32.0655, 32.8124, 32.0773, 32.7775, 31.8054, 32.0873, 31.8231, 32.0895,
          32.1678, 31.7708, 31.8761, 31.6765, 32.1089, 32.0162, 32.0630, 31.7083, 31.7267, 32.2907
        };
            double[] CallLongitudes = {
         34.7704, 34.8221, 34.9890, 34.7915, 35.2137, 34.7839, 34.8573, 34.8874, 34.7909, 34.9494,
         34.7631, 34.8036, 34.7862, 34.7927, 34.6292, 34.7657, 34.7894, 34.8360, 34.8040, 34.6489,
         34.6415, 34.8433, 34.7589, 34.8352, 34.8195, 34.7629, 34.8792, 34.8299, 34.8837, 34.7956,
         34.9216, 34.7720, 34.7700, 34.8364, 34.7797, 34.8741, 34.8454, 34.7950, 34.7721, 34.8154,
         34.8361, 34.9046, 34.7964, 34.7533, 34.7831, 34.7724, 34.8302, 34.7789, 34.7819, 34.8065
        };

            TypeOfCall[] TypeOfCalls =
         {
            TypeOfCall.ToPackageFood,
            TypeOfCall.ToPrepareFood,
            TypeOfCall.ToCarryFood,
            TypeOfCall.ToCommunityCookingNights,
            TypeOfCall.ToDonateRawMaterials,
            TypeOfCall.ToPackageFood,
            TypeOfCall.ToPrepareFood,
            TypeOfCall.ToCarryFood,
            TypeOfCall.ToCommunityCookingNights,
            TypeOfCall.ToDonateRawMaterials,
            TypeOfCall.ToPackageFood,
            TypeOfCall.ToPrepareFood,
            TypeOfCall.ToCarryFood,
            TypeOfCall.ToCommunityCookingNights,
            TypeOfCall.ToDonateRawMaterials,
            TypeOfCall.ToPackageFood,
            TypeOfCall.ToPrepareFood,
            TypeOfCall.ToCarryFood,
            TypeOfCall.ToCommunityCookingNights,
            TypeOfCall.ToDonateRawMaterials,
            TypeOfCall.ToPackageFood,
            TypeOfCall.ToPrepareFood,
            TypeOfCall.ToCarryFood,
            TypeOfCall.ToCommunityCookingNights,
            TypeOfCall.ToDonateRawMaterials,
            TypeOfCall.ToPackageFood,
            TypeOfCall.ToPrepareFood,
            TypeOfCall.ToCarryFood,
            TypeOfCall.ToCommunityCookingNights,
            TypeOfCall.ToDonateRawMaterials,
            TypeOfCall.ToPackageFood,
            TypeOfCall.ToPrepareFood,
            TypeOfCall.ToCarryFood,
            TypeOfCall.ToCommunityCookingNights,
            TypeOfCall.ToDonateRawMaterials,
            TypeOfCall.ToPackageFood,
            TypeOfCall.ToPrepareFood,
            TypeOfCall.ToCarryFood,
            TypeOfCall.ToCommunityCookingNights,
            TypeOfCall.ToDonateRawMaterials,
            TypeOfCall.ToPackageFood,
            TypeOfCall.ToPrepareFood,
            TypeOfCall.ToCarryFood,
            TypeOfCall.ToCommunityCookingNights,
            TypeOfCall.ToDonateRawMaterials,
            TypeOfCall.ToPackageFood,
            TypeOfCall.ToPrepareFood,
            TypeOfCall.ToCarryFood,
            TypeOfCall.ToCommunityCookingNights,
            TypeOfCall.ToDonateRawMaterials
        };

            int i = 0;
            foreach (DO.TypeOfCall c_type in TypeOfCalls)
            {
                callOpeningTime = systemTime.AddMinutes(-rand.Next(1, 1000));

                int durationMinutes = rand.Next(30, 180);
                callMaxFinishTime = callOpeningTime.AddMinutes(durationMinutes);

                s_dal!.Call.Create(new DO.Call(
                    TypeOfCall: c_type,
                    Address: CallAddresses[i],
                    Longitude: CallLongitudes[i],
                    Latitude: CallLatitudes[i],
                    OpeningTime: callOpeningTime,
                    MaxTimeForClosing: callMaxFinishTime,
                    CallDescription: CallDescriptions[i]
                ));
                i++;
            }
        }

        /// <summary>
        /// Creates volunteers with random data.
        /// Generates a list of volunteers with IDs, names, addresses, and phone numbers,
        /// and inserts them into the database.
        /// </summary>
        private static void createVolunteer()
        {
            Random rand = new Random();
            int[] VolunteerIds = {
                215639212, 215525197, 328118245, 214821423, 326365574,
                328128061, 327770442, 327548194, 215086950, 215215690,
                215252370, 328177191, 328183934, 327786612, 328306550,
                327820429, 328276332, 328304944, 215238023, 327796678};

            string[] VolunteerNames = {
                "David Cohen", "Yaara Levy", "Matan Mizrahi", "Tamar Shapiro", "Oren Ben-David",
                "Yael Peretz", "Eliav Rosen", "Michal Goldstein", "Amir Kaplan", "Lior Avrahami",
                "Shani Shani", "Noa Biton", "Yonatan Katz", "Ruth Ziv", "Doron Barak",
                "Yaara Nachmias", "Roi Tzukrel", "Michal Adler", "Shir Fridman", "Erez Dayan"
            };

            string[] VolunteerAddresses = {
              "120 Dizengoff St, Tel Aviv",
              "15 Herzl St, Haifa",
              "30 Jaffa St, Jerusalem",
              "22 Rothschild Blvd, Tel Aviv",
              "10 Derech HaShalom, Ramat Gan",
              "1 HaNassi Blvd, Haifa",
              "45 HaAtzmaut Blvd, Ashdod",
              "88 Allenby St, Tel Aviv",
              "12 Ben Gurion Blvd, Bat Yam",
              "20 King George St, Jerusalem",
              "132 Begin Road, Tel Aviv",
              "8 Keren Hayesod St, Jerusalem",
              "99 Ben Yehuda St, Tel Aviv",
              "25 Weizmann St, Kfar Saba",
              "14 Golani Brigade St, Afula",
              "33 Yehuda Halevi St, Tel Aviv",
              "77 HaNassi Blvd, Haifa",
              "19 Arlozorov St, Tel Aviv",
              "96 HaHashmonaim St, Tel Aviv",
              "12 Menachem Begin Blvd, Rishon LeZion"
           };
            double[] Latitudes = {
              32.0736, 32.0739, 32.8194, 32.0702, 31.7702, 32.0782, 32.3212, 32.0951, 32.1575, 29.5596,
              32.1749, 32.0926, 32.0814, 32.1311, 31.7665, 32.0509, 32.0354, 31.9737, 32.0656, 31.6730
           };
            double[] Longitudes = {
              34.7704, 34.8221, 34.9890, 34.7915, 35.2137, 34.7839, 34.8573, 34.8874, 34.7909, 34.9494,
              34.7631, 34.8036, 34.7862, 34.7927, 34.6292, 34.7657, 34.7894, 34.8360, 34.8040, 34.6489
           };

            static string GeneratePassword(int length)
            {
                if (length < 4)
                    throw new ArgumentException("Password length must be at least 4 to meet complexity requirements.");

                const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string lowercase = "abcdefghijklmnopqrstuvwxyz";
                const string digits = "0123456789";
                const string specialChars = "!@#$%^&*()-_=+[]{}|;:'\",.<>?/";

                using var rng = RandomNumberGenerator.Create();
                char[] password = new char[length];

                // Ensure at least one character from each required category
                password[0] = GetRandomChar(uppercase, rng);
                password[1] = GetRandomChar(lowercase, rng);
                password[2] = GetRandomChar(digits, rng);
                password[3] = GetRandomChar(specialChars, rng);

                // Fill the rest of the password with random characters from all categories
                string allChars = uppercase + lowercase + digits + specialChars;
                for (int i = 4; i < length; i++)
                {
                    password[i] = GetRandomChar(allChars, rng);
                }

                // Shuffle the password
                password = password.OrderBy(_ => GetRandomInt(rng)).ToArray();

                return new string(password);
            }

            static char GetRandomChar(string chars, RandomNumberGenerator rng)
            {
                byte[] randomByte = new byte[1];
                do
                {
                    rng.GetBytes(randomByte);
                } while (randomByte[0] >= 256 - (256 % chars.Length));

                return chars[randomByte[0] % chars.Length];
            }

            static int GetRandomInt(RandomNumberGenerator rng)
            {
                byte[] randomBytes = new byte[4];
                rng.GetBytes(randomBytes);
                return BitConverter.ToInt32(randomBytes, 0) & int.MaxValue;
            }

            static string HashPasswordSHA256(string password)
            {
                using SHA256 sha256 = SHA256.Create();
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }



            int i = 0;
            foreach (string name in VolunteerNames)
            {
                Volunteer newVolunteer = new Volunteer
                {
                    ID = VolunteerIds[i],
                    Name = name,
                    Phone = $"0{rand.Next(100000000, 999999999)}",
                    Email = name.Replace(" ", "").ToLower() + "@gmail.com",
                    Address = VolunteerAddresses[i],
                    Latitude = Latitudes[i],
                    Longitude = Longitudes[i++],
                    Role = Role.Volunteer,
                    IsActive = true,
                    DistanceType = (DistanceType)rand.Next(Enum.GetValues(typeof(DistanceType)).Length),
                    MaxDistanceForCall = rand.Next(0, 100000)
                };

                //if (s_dalVolunteer!.Read(newVolunteer.ID) == null){s_dalVolunteer.Create(newVolunteer);}//stage 1
                if (s_dal!.Volunteer.Read(newVolunteer.ID) == null) { s_dal!.Volunteer.Create(newVolunteer); }//stage 2

            }
        }

        /// <summary>
        /// Creates assignments by matching volunteers with calls.
        /// </summary>
        private static void createAssignments()
        {
            Random rand = new Random();
            var calls = s_dal!.Call.ReadAll().ToList();
            var volunteers = s_dal!.Volunteer.ReadAll().ToList();
            var callsWithAssignment = calls.Skip((int)(calls.Count * 0.2)).ToList();

            foreach (Call? call in callsWithAssignment)
            {
                Volunteer randomVolunteer;
                if (volunteers.Count > 0)
                {
                    randomVolunteer = volunteers[rand.Next(volunteers.Count)]!;
                    if (randomVolunteer == null || randomVolunteer.ID == 0)
                        throw new Exception("Selected volunteer is invalid or has ID 0.");
                }
                else
                {
                    throw new Exception("No volunteers available.");
                }
                TimeSpan duration;
                if (call!.MaxTimeForClosing.HasValue)
                    duration = call.MaxTimeForClosing.Value - call.OpeningTime;
                else
                    duration = TimeSpan.Zero;

                double randomMinutes = rand.NextDouble() * duration.TotalMinutes;
                DateTime randomStartTime = call.OpeningTime.AddMinutes(randomMinutes);

                DateTime? randomEndTime;

                if (!call.MaxTimeForClosing.HasValue)
                {
                    if (rand.NextDouble() < 0.5)
                    {
                        double maxMinutes = (s_dal.Config.Clock - randomStartTime).TotalMinutes;
                        if (maxMinutes > 0)
                        {
                            double endMinutes = rand.NextDouble() * maxMinutes;
                            randomEndTime = randomStartTime.AddMinutes(endMinutes);
                        }
                        else
                        {
                            randomEndTime = randomStartTime;
                        }
                    }
                    else
                    {
                        randomEndTime = null;
                    }
                }
                else if (call.MaxTimeForClosing.Value < s_dal.Config.Clock)
                {
                    randomEndTime = s_dal.Config.Clock;
                }
                else
                {
                    if (rand.NextDouble() < 0.5)
                    {
                        DateTime maxAllowed = call.MaxTimeForClosing.Value < s_dal.Config.Clock
                            ? call.MaxTimeForClosing.Value
                            : s_dal.Config.Clock;

                        double maxMinutes = (maxAllowed - randomStartTime).TotalMinutes;
                        if (maxMinutes > 0)
                        {
                            double endMinutes = rand.NextDouble() * maxMinutes;
                            randomEndTime = randomStartTime.AddMinutes(endMinutes);
                        }
                        else
                        {
                            randomEndTime = randomStartTime;
                        }
                    }
                    else
                    {
                        randomEndTime = null;
                    }
                }

                TypeOfFinishTreatment? endType = null;
                if (randomEndTime == null)
                    endType = null;
                else if (call.MaxTimeForClosing.HasValue && call.MaxTimeForClosing < s_dal.Config.Clock)
                {
                    endType = TypeOfFinishTreatment.OutOfRangeCancellation;
                }
                else
                {
                    double randomPercentage = rand.NextDouble();
                    if (randomPercentage < 0.33)
                        endType = TypeOfFinishTreatment.Treated;
                    else if (randomPercentage < 0.66)
                        endType = TypeOfFinishTreatment.SelfCancellation;
                    else
                        endType = TypeOfFinishTreatment.ManagerCancellation;
                }
                s_dal!.Assignment.Create(new Assignment
                {
                    CallId = call.ID,
                    VolunteerId = randomVolunteer.ID,
                    EntryTimeForTreatment = randomStartTime,
                    EndTimeForTreatment = randomEndTime,
                    TypeOfFinishTreatment = endType
                });
            }
        }
        private static void createSuperAdmin()
        {
            if (s_dal!.Volunteer.Read(214323222) != null)
                return;

            string password = "Qq1!qwertyui";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            string hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            Volunteer superAdmin = new Volunteer
            {
                ID = 214323222,
                Name = "Super Admin",
                Phone = "052-0000000",
                Email = "rachlios2005@gmail.com",
                Address = "mevo livna 5",
                Latitude = 32.1,
                Longitude = 34.8,
                Password = hashString,
                Role = Role.Manager,
                IsActive = true,
                DistanceType = DistanceType.DrivingDistance,
                MaxDistanceForCall = 500000
            };
            s_dal.Volunteer.Create(superAdmin);
        }
    }
}