namespace Helpers;

internal static class Tools
{
    /// <summary>
    /// Generates a string with all properties and their values of the given object. /// </summary>
    /// <typeparam name="T">The type of the object being processed.</typeparam>
    /// <param name="t">The object to process.</param>
    /// <returns>A string representing all properties and their values.</returns>
    public static string ToStringProperty<T>(this T t)
    {
        if (t == null)
            return "Object is null";
        // Using reflection to get all public properties of the object
        var properties = t.GetType().GetProperties();
        // Build the output string by iterating over the properties
        var result = new System.Text.StringBuilder();
        result.AppendLine($"Type: {t.GetType().Name}");
        foreach (var prop in properties)
        {
            var value = prop.GetValue(t) ?? "null";
            result.AppendLine($"{prop.Name}: {value}");
        }
        return result.ToString();
    }
}
//using BO;
//using Newtonsoft.Json.Linq;
//namespace Helpers;

//internal static class Tools
//{
//    public static string ToStringProperty<T>(this T t)
//    {
//        return "t";//change
//    }
//    private static readonly DalApi.IDal _dal = DalApi.Factory.Get; //stage 4
//    private static readonly string apiUrl = "https://geocode.maps.co/search?q=address&api_key={1}";
//    private static readonly string apiKey = "67988b4fd5f89506953512sdu4aa70a";

//    public static double CalculateDistance(object latitude1, object longitude1, double latitude2, double longitude2)
//    {
//        // המרת פרמטרים מסוג object ל-double
//        if (!double.TryParse(latitude1?.ToString(), out double lat1) ||
//            !double.TryParse(longitude1?.ToString(), out double lon1))
//        {
//            throw new ArgumentException("Invalid latitude or longitude values.");
//        }

//        const double EarthRadiusKm = 6371; // רדיוס כדור הארץ בקילומטרים

//        // המרת מעלות לרדיאנים
//        double lat1Rad = DegreesToRadians(lat1);
//        double lon1Rad = DegreesToRadians(lon1);
//        double lat2Rad = DegreesToRadians(latitude2);
//        double lon2Rad = DegreesToRadians(longitude2);

//        // חישוב ההפרשים
//        double deltaLat = lat2Rad - lat1Rad;
//        double deltaLon = lon2Rad - lon1Rad;

//        // נוסחת Haversine
//        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
//                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
//                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
//        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

//        // חישוב המרחק
//        double distance = EarthRadiusKm * c;
//        return distance;
//    }

//    public static double DegreesToRadians(double degrees)
//    {
//        return degrees * Math.PI / 180;
//    }

//    public static CallStatusEnum CalculateStatus(DO.Assignment currentAssignment, DO.Call callDetails, int timeMarginMinutes)
//    {
//        if (currentAssignment.EndTime.HasValue)
//        {
//            // השיחה הושלמה, אין צורך לעדכן את המצב
//            return CallStatusEnum.InProgress; // אנחנו מחזירים את המצב InProgress אם השיחה הושלמה
//        }
//        // חישוב הזמן הנותר לסיום השיחה
//        DateTime maxFinishTime = callDetails.MaxTimeToFinish;
//        // הזמן הנוכחי
//        DateTime currentTime = DateTime.Now;
//        // חישוב ההפרש בין הזמן הנוכחי לזמן הסיום המקסימלי של השיחה
//        TimeSpan timeDifference = maxFinishTime - currentTime;
//        // אם הזמן הנותר לשיחה הוא פחות מ-30 דקות (סיכון), נחזיר AtRisk
//        if (timeDifference.TotalMinutes <= timeMarginMinutes)
//        {
//            return CallStatusEnum.AtRisk;
//        }

//        // אם הזמן נותר יותר מ-30 דקות, אז השיחה עדיין בתהליך
//        return CallStatusEnum.InProgress;
//    }
//    public static (double? Latitude, double? Longitude) GetCoordinatesFromAddress(string address)
//    {
//        if (string.IsNullOrWhiteSpace(address))
//        {
//            throw new Exception(address); // חריגה אם הכתובת לא תקינה
//        }

//        try
//        {
//            // יצירת ה-URL לפנייה ל-API
//            string url = string.Format(apiUrl, Uri.EscapeDataString(address), apiKey);

//            using (HttpClient client = new HttpClient())
//            {

//                HttpResponseMessage response = client.GetAsync(url).Result;

//                if (response.IsSuccessStatusCode)
//                {
//                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
//                    JArray jsonArray = JArray.Parse(jsonResponse);
//                    if (jsonArray.Count > 0)
//                    {
//                        var firstResult = jsonArray[0];
//                        double latitude = (double)firstResult["lat"];
//                        double longitude = (double)firstResult["lon"];
//                        return (latitude, longitude);
//                    }
//                    else
//                    {
//                        throw new Exception(address); // חריגה אם לא נמצאה גיאוקולציה
//                    }
//                }
//                else
//                {
//                    throw new Exception($"API request failed with status code: {response.StatusCode}"); // חריגה אם הבקשה נכשלה
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            // אם קרתה שגיאה כלשהי, זורקים חריגה עם פרטי השגיאה
//            throw new Exception($"Error occurred while fetching coordinates for the address. {ex.Message}");
//        }
//    }
//}

