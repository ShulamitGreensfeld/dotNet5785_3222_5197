using BO;
using System.Text.Json;
namespace Helpers;
using System.Collections;
//using System.Net;
//using System.Net.Mail;
//using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;


internal static class Tools
{
    private static readonly DalApi.IDal _dal = DalApi.Factory.Get; //stage 4

    /// <summary>
    /// Converts an object to a string representation of its properties.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="t">The object to convert to a string.</param>
    /// <returns>A string representing the object's properties.</returns>
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

    /// <summary>
    /// Calculates the distance between two geographical points using the Haversine formula.
    /// </summary>
    /// <param name="latitude1">The latitude of the first point.</param>
    /// <param name="longitude1">The longitude of the first point.</param>
    /// <param name="latitude2">The latitude of the second point.</param>
    /// <param name="longitude2">The longitude of the second point.</param>
    /// <returns>The distance in kilometers between the two points.</returns>
    public static double CalculateDistance(object latitude1, object longitude1, double latitude2, double longitude2)
    {
        // Convert object parameters to double
        if (!double.TryParse(latitude1?.ToString(), out double lat1) ||
            !double.TryParse(longitude1?.ToString(), out double lon1))
        {
            throw new ArgumentException("Invalid latitude or longitude values.");
        }

        const double EarthRadiusKm = 6371; // Radius of the Earth in kilometers

        // Convert degrees to radians
        double lat1Rad = DegreesToRadians(lat1);
        double lon1Rad = DegreesToRadians(lon1);
        double lat2Rad = DegreesToRadians(latitude2);
        double lon2Rad = DegreesToRadians(longitude2);

        // Calculate the differences
        double deltaLat = lat2Rad - lat1Rad;
        double deltaLon = lon2Rad - lon1Rad;

        // Haversine formula
        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Calculate the distance
        double distance = EarthRadiusKm * c;
        return distance;
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The angle in radians.</returns>
    public static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    /// <summary>
    /// Retrieves the coordinates (latitude and longitude) for a given address using a geocoding API.
    /// </summary>
    /// <param name="address">The address for which to retrieve coordinates.</param>
    /// <returns>A tuple containing the latitude and longitude of the address.</returns>
    public static (double, double) GetCoordinatesFromAddress(string address)
    {
        string apiKey = "PK.83B935C225DF7E2F9B1ee90A6B46AD86";
        using var client = new HttpClient();
        string url = $"https://us1.locationiq.com/v1/search.php?key={apiKey}&q={Uri.EscapeDataString(address)}&format=json";

        var response = client.GetAsync(url).GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
            throw new Exception("Invalid address or API error.");

        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
            throw new Exception("Address not found.");

        var root = doc.RootElement[0];

        // Convert values to double
        double latitude = double.Parse(root.GetProperty("lat").GetString());
        double longitude = double.Parse(root.GetProperty("lon").GetString());

        return (latitude, longitude);
    }

    /// <summary>
    /// Hashes a password using SHA256.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password as a string.</returns>
    public static string HashPassword(string password)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// Stores a volunteer's name and hashed password in an XML file.
    /// </summary>
    /// <param name="volunteerName">The name of the volunteer.</param>
    /// <param name="hashedPassword">The hashed password to store.</param>
    /// <param name="filePath">The file path where the data should be saved.</param>
    public static void StorePasswordInXml(string volunteerName, string hashedPassword, string filePath)
    {
        XDocument doc;
        if (File.Exists(filePath))
        {
            doc = XDocument.Load(filePath);
        }
        else
        {
            doc = new XDocument(new XElement("Volunteers"));
        }

        XElement volunteerElement = new XElement("Volunteer",
            new XElement("Name", volunteerName),
            new XElement("Password", hashedPassword)
        );

        doc.Root.Add(volunteerElement);
        doc.Save(filePath);
    }
}
