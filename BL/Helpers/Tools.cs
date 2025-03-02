using System.Collections;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;


namespace Helpers;

internal static class Tools
{
    public static string ToStringProperty<T>(this T t)
    {
        if (t == null)
            return "";

        var result = new StringBuilder();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(t);
            if (value is IEnumerable enumerable && value is not string)
            {
                result.Append($"{property.Name}: [");
                foreach (var item in enumerable)
                {
                    result.Append($"{item}, ");
                }
                result.Append("], ");
            }
            else
            {
                result.Append($"{property.Name}: {value}, ");
            }
        }
        return result.ToString().TrimEnd(',', ' ');
    }

    public static double CalculateDistance(double? lat1, double? lon1, double lat2, double lon2)
    {
        var lat1Value = lat1 ?? 0;
        var lon1Value = lon1 ?? 0;

        var r = 6371;
        var dLat = (lat2 - lat1Value) * Math.PI / 180;
        var dLon = (lon2 - lon1Value) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1Value * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return r * c;
    }

    private static readonly string apiKey = "679a326087d8e538214620ibs5de71a";
    private static readonly string apiUrl = "https://geocode.maps.co/search?q={0}&format=json&api_key={1}";
    /// <summary>
    /// Retrieves coordinates (latitude and longitude) for a given address.
    /// If the address is invalid or the API request fails, an appropriate exception is thrown.
    /// </summary>
    /// <param name="address">The address for which coordinates are requested.</param>
    /// <returns>A tuple containing latitude and longitude of the address.</returns>
    /// <exception cref="InvalidAddressException">Thrown when the address is invalid or cannot be processed.</exception>
    /// <exception cref="ApiRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="GeolocationNotFoundException">Thrown when no geolocation is found for the address.</exception>
    public static (double? latitude, double? longitude) GetCoordinatesFromAddress(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            throw new ArgumentException("הכתובת אינה תקינה.");
        }

        try
        {
            // יצירת חיבור ל-API
            using (HttpClient client = new HttpClient())
            {
                // מבצע קריאה סינכרונית (בלי async/await)
                string url = string.Format(apiUrl, Uri.EscapeDataString(address), apiKey);
                var response = client.GetStringAsync(url).Result;  // קריאה סינכרונית

                // ניתוח התשובה ב-JSON
                var jsonResponse = JArray.Parse(response);
                if (jsonResponse.Count > 0)
                {
                    var firstResult = jsonResponse[0];
                    double latitude = firstResult.Value<double>("lat");
                    double longitude = firstResult.Value<double>("lon");

                    return (latitude, longitude);
                }
                else
                {
                    throw new Exception("לא נמצאו קואורדינטות לכתובת הזו.");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"שגיאה בקבלת קואורדינטות: {ex.Message}");
        }
    }
}