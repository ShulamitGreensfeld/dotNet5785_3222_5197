using DalApi;

namespace Helpers;
//לל
internal static class VolunteerManager
{
    private static IDal s_dal = Factory.Get; //stage 4
    /// <summary>
    /// מחזירה קואורדינטות של כתובת (רוחב ואורך) באמצעות API של LocationIQ.
    /// </summary>
    public static (double Latitude, double Longitude) GetCoordinates(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("כתובת לא תקינה");

        using var client = new HttpClient();
        string apiKey = "YOUR_LOCATIONIQ_API_KEY";  // השתמש ב-API key שלך
        string url = $"https://us1.locationiq.com/v1/search.php?key={apiKey}&q={Uri.EscapeDataString(address)}&format=json";

        var response = client.GetStringAsync(url).Result;
        var json = System.Text.Json.JsonSerializer.Deserialize<dynamic>(response);
        if (json == null || json[0] == null)
            throw new Exception("כתובת לא נמצאה");

        return (double.Parse(json[0]["lat"].ToString()), double.Parse(json[0]["lon"].ToString()));
    }

    /// <summary>
    /// מחשבת את המרחק האווירי בין שתי כתובות.
    /// </summary>
    public static double CalculateAirDistance(string address1, string address2)
    {
        var (lat1, lon1) = GetCoordinates(address1);
        var (lat2, lon2) = GetCoordinates(address2);

        double R = 6371; // רדיוס כדור הארץ בק"מ
        double dLat = DegreesToRadians(lat2 - lat1);
        double dLon = DegreesToRadians(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
    public static void ChooseCallForVolunteer()
    {

    }

}
