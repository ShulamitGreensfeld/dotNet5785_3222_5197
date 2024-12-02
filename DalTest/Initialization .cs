using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DalTest
{
    public static class Initialization
    {
        private static ICall? s_dalCall;
        private static IVolunteer? s_dalVolunteer;
        private static IAssignment? s_dalAssignment;
        private static IConfig? s_dalConfig;
        private static readonly Random s_rand = new();

        /// <summary>
        /// Initializes the database with random data.
        /// </summary>
        public static void Do(ICall? dalCall, IAssignment? dalAssignment, IVolunteer? dalVolunteer, IConfig? dalConfig)
        {
            s_dalCall = dalCall ?? throw new NullReferenceException("DAL for Calls cannot be null!");
            s_dalAssignment = dalAssignment ?? throw new NullReferenceException("DAL for Assignments cannot be null!");
            s_dalVolunteer = dalVolunteer ?? throw new NullReferenceException("DAL for Volunteers cannot be null!");
            s_dalConfig = dalConfig ?? throw new NullReferenceException("DAL for Config cannot be null!");

            Console.WriteLine("Resetting configuration and clearing data...");
            s_dalConfig.Reset();
            s_dalCall.DeleteAll();
            s_dalVolunteer.DeleteAll();
            s_dalAssignment.DeleteAll();

            Console.WriteLine("Creating data...");
            createCall();
            createVolunteer();
            createAssignments();
            Console.WriteLine("Initialization completed successfully!");
        }

        /// <summary>
        /// Creates calls with realistic data.
        /// </summary>
        private static void createCall()
        {
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
          "10 Nordau Boulevard, Tel Aviv",
          "5 Bialik Street, Ramat Gan",
          "12 HaAtzmaut Street, Haifa",
          "18 Eilat Street, Tel Aviv",
          "22 David HaMelech Street, Jerusalem",
          "30 Yehuda HaLevi Street, Tel Aviv",
          "15 HaRav Kook Street, Netanya",
          "19 HaShomer Street, Petah Tikva",
          "9 Shderot HaGalim, Herzliya",
          "7 HaArava Street, Eilat",
          "4 HaHistadrut Boulevard, Kfar Saba",
          "27 HaTzafon Street, Holon",
          "33 Jabotinsky Boulevard, Ramat Gan",
          "25 Alkalai Street, Tel Aviv",
          "6 HaAliya Street, Acre",
          "20 Jerusalem Boulevard, Bat Yam",
          "2 HaTsiyonut Street, Rishon LeZion",
          "14 HaNasi Harishon Boulevard, Rehovot",
          "11 King Solomon Street, Ashkelon",
          "8 Harel Street, Modi'in",
          "35 HaZamir Street, Ashdod",
          "31 Herzl Boulevard, Tiberias",
          "5 HaRav Herzog Street, Haifa",
          "13 HaPalmach Boulevard, Be'er Sheva",
          "40 Bialik Street, Raanana",
          "3 Emek Yizrael Street, Afula",
          "50 Shlomo HaMelech Street, Kiryat Shmona",
          "7 HaEmek Street, Nazareth",
          "12 Shderot Haatzmaut, Hadera",
          "29 HaMa'ayan Street, Kiryat Gat",
          "18 HaNegev Boulevard, Sderot",
          "45 HaShalom Boulevard, Givatayim",
          "22 Yitzhak Rabin Street, Tel Aviv",
          "19 Yehuda Maccabi Street, Haifa",
          "6 HaTanaim Street, Ramat HaSharon",
          "38 HaHoresh Street, Rosh HaAyin",
          "14 Beit HaArava Street, Jerusalem",
          "10 HaChavatzelet Street, Netanya",
          "8 HaNarkisim Street, Ashdod",
          "31 HaBanim Boulevard, Holon",
          "3 HaGvura Street, Bat Yam",
          "21 HaGeula Street, Herzliya",
          "50 HaMasger Street, Ramat Gan",
          "17 HaRakefet Street, Eilat",
          "9 HaDekel Street, Tel Aviv",
          "13 HaSharon Boulevard, Hadera",
          "7 Shderot HaZayit, Be'er Sheva",
          "23 HaPisga Street, Modi'in",
          "4 HaTavor Street, Tiberias",
          "46 HaTavor Street, Tiberias"
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

            DateTime start = s_dalConfig!.Clock.AddHours(-7);
            int range = (int)(s_dalConfig.Clock - start).TotalMinutes;

            for (int i = 0; i < 50; i++)
            {
                int index = s_rand.Next(CallDescriptions.Length);

                DateTime openingTime = start.AddMinutes(-s_rand.Next(range));

                Call newCall = new Call(
                    TypeOfCall: TypeOfCalls[index],
                    Address: CallAddresses[index],
                    Longitude: CallLongitudes[index],
                    Latitude: CallLatitudes[index],
                    OpeningTime: openingTime,
                    MaxTimeForClosing: start.AddMinutes(s_rand.Next(1, 720)),
                    CallDescription: CallDescriptions[index]
                );

                if (s_dalCall!.Read(newCall.ID) == null)
                {
                    s_dalCall.Create(newCall);
                }
            }
        }

        /// <summary>
        /// Creates volunteers with random data.
        /// </summary>
        private static void createVolunteer()
        {

            string[] VolunteerNames = {
    "David Cohen", "Yaara Levy", "Matan Mizrahi", "Tamar Shapiro", "Oren Ben-David",
    "Yael Peretz", "Eliav Rosen", "Michal Goldstein", "Amir Kaplan", "Lior Avrahami",
    "Shani Shani", "Noa Biton", "Yonatan Katz", "Ruth Ziv", "Doron Barak",
    "Yaara Nachmias", "Roi Tzukrel", "Michal Adler", "Shir Fridman", "Erez Dayan"
};

            string[] VolunteerAddresses = {
         "10 Nordau Boulevard, Tel Aviv", "5 Bialik Street, Ramat Gan", "12 HaAtzmaut Street, Haifa", "18 Eilat Street, Tel Aviv",
         "22 David HaMelech Street, Jerusalem", "30 Yehuda HaLevi Street, Tel Aviv", "15 HaRav Kook Street, Netanya",
         "19 HaShomer Street, Petah Tikva", "9 Shderot HaGalim, Herzliya", "7 HaArava Street, Eilat", "4 HaHistadrut Boulevard, Kfar Saba",
         "27 HaTzafon Street, Holon", "33 Jabotinsky Boulevard, Ramat Gan", "25 Alkalai Street, Tel Aviv", "6 HaAliya Street, Acre",
         "20 Jerusalem Boulevard, Bat Yam", "2 HaTsiyonut Street, Rishon LeZion", "14 HaNasi Harishon Boulevard, Rehovot",
         "11 King Solomon Street, Ashkelon", "8 Harel Street, Modi'in"
       };

            string[] VolunteerPhoneNumbers = {
         "052-1234567", "052-2345678", "052-3456789", "052-4567890", "052-5678901", "052-6789012", "052-7890123",
         "052-8901234", "052-9012345", "052-0123456", "052-1239876", "052-2349876", "052-3459876", "052-4569876",
         "052-5679876", "052-6789876", "052-7899876", "052-8909876", "052-9019876", "052-0129876"
       };
            double[] Latitudes = {
          32.0736, 32.0739, 32.8194, 32.0702, 31.7702, 32.0782, 32.3212, 32.0951, 32.1575, 29.5596,
          32.1749, 32.0926, 32.0814, 32.1311, 31.7665, 32.0509, 32.0354, 31.9737, 32.0656, 31.6730
       };
            double[] Longitudes = {
          34.7704, 34.8221, 34.9890, 34.7915, 35.2137, 34.7839, 34.8573, 34.8874, 34.7909, 34.9494,
          34.7631, 34.8036, 34.7862, 34.7927, 34.6292, 34.7657, 34.7894, 34.8360, 34.8040, 34.6489
       };

            for (int i = 0; i < VolunteerNames.Length; i++)
            {
                Volunteer newVolunteer = new Volunteer
                {
                    ID = s_rand.Next(100000000, 999999999),
                    Name = VolunteerNames[i],
                    Phone = VolunteerPhoneNumbers[i],
                    Email = $"{VolunteerNames[i].Replace(" ", "").ToLower()}@example.com",
                    Address = VolunteerAddresses[i],
                    Latitude = Latitudes[i],
                    Longitude = Longitudes[i],
                    Role = Role.Volunteer,
                    IsActive = true,
                    DistanceType = (DistanceType)s_rand.Next(Enum.GetValues(typeof(DistanceType)).Length),
                    MaxDistanceForCall = s_rand.Next(1, 100)
                };

                if (s_dalVolunteer!.Read(newVolunteer.ID) == null)
                {
                    s_dalVolunteer.Create(newVolunteer);
                }
            }
        }

        /// <summary>
        /// Creates assignments by matching volunteers with calls.
        /// </summary>
        private static void createAssignments()
        {
            var volunteers = s_dalVolunteer!.ReadAll();
            var calls = s_dalCall!.ReadAll();

            foreach (var call in calls)
            {
                if (call.MaxTimeForClosing == null || !volunteers.Any())
                {
                    continue;
                }

                Volunteer randomVolunteer = volunteers[s_rand.Next(volunteers.Count)];
                DateTime minTime = call.OpeningTime;
                DateTime maxTime = (DateTime)call.MaxTimeForClosing;
                if (minTime >= maxTime) continue;

                DateTime randomTime = minTime.AddMinutes(s_rand.Next((int)(maxTime - minTime).TotalMinutes));
                TypeOfFinishTreatment finishType = (TypeOfFinishTreatment)s_rand.Next(Enum.GetValues(typeof(TypeOfFinishTreatment)).Length);

                if (s_dalAssignment!.ReadAll().Any(a => a.CallId == call.ID && a.VolunteerId == randomVolunteer.ID))
                {
                    continue;
                }

                s_dalAssignment.Create(new Assignment
                {
                    CallId = call.ID,
                    VolunteerId = randomVolunteer.ID,
                    EntryTimeForTreatment = randomTime,
                    EndTimeForTreatment = randomTime.AddHours(2),
                    TypeOfFinishTreatment = finishType
                });
            }
        }
    }
}