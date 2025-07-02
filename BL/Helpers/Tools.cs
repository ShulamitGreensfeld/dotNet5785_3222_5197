using System.Net.Mail;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text;

internal static class Tools
{
    private static readonly DalApi.IDal _dal = DalApi.Factory.Get; //stage 4

    // Property to store the selected distance type
    private static BO.Enums.DistanceTypes _selectedDistanceType = BO.Enums.DistanceTypes.aerial_distance;

    /// <summary>
    /// Sets the selected distance type.
    /// </summary>
    /// <param name="distanceType">The distance type to set.</param>
    public static void SetDistanceType(BO.Enums.DistanceTypes distanceType)
    {
        _selectedDistanceType = distanceType;
    }

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
            if (value is IEnumerable<object> enumerable && value is not string)
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
    /// Asynchronously calculates the distance between two geographical points based on the specified distance type.
    /// </summary>
    /// <param name="type">The type of distance calculation (aerial, walking, or driving).</param>
    /// <param name="latitudeV">The latitude of the first point.</param>
    /// <param name="longitudeV">The longitude of the first point.</param>
    /// <param name="latitudeC">The latitude of the second point.</param>
    /// <param name="longitudeC">The longitude of the second point.</param>
    /// <returns>The calculated distance in kilometers.</returns>
    public static async Task<double> CalculateDistanceAsync(BO.Enums.DistanceTypes type, double latitudeV, double longitudeV, double latitudeC, double longitudeC)
    {
        return type switch
        {
            BO.Enums.DistanceTypes.aerial_distance => CalculateDistance(latitudeV, longitudeV, latitudeC, longitudeC),
            BO.Enums.DistanceTypes.walking_distance => await GetRouteDistanceAsync(latitudeV, longitudeV, latitudeC, longitudeC, "walking"),
            BO.Enums.DistanceTypes.driving_distance => await GetRouteDistanceAsync(latitudeV, longitudeV, latitudeC, longitudeC, "driving"),
            _ => throw new ArgumentException("Invalid distance type", nameof(type))
        };
    }

    /// <summary>
    /// Asynchronously retrieves the distance between two geographical points using the TomTom API for a specified travel mode.
    /// </summary>
    /// <param name="latitudeV">The latitude of the starting point.</param>
    /// <param name="longitudeV">The longitude of the starting point.</param>
    /// <param name="latitudeC">The latitude of the destination.</param>
    /// <param name="longitudeC">The longitude of the destination.</param>
    /// <param name="travelMode">The mode of travel (e.g., pedestrian, car).</param>
    /// <returns>The calculated route distance in kilometers, or double.MaxValue if an error occurs.</returns>
    private static async Task<double> GetRouteDistanceAsync(
    double latitudeV, double longitudeV,
    double latitudeC, double longitudeC,
    string travelMode)
    {
        using HttpClient client = new HttpClient();

        string apiKey = "AIzaSyBA-EmNEQuxNPdTVE4BxFV-pzukM7x6pAM";
        string url = $"https://maps.googleapis.com/maps/api/distancematrix/json" +
                     $"?origins={latitudeV},{longitudeV}&destinations={latitudeC},{longitudeC}&mode={travelMode}&key={apiKey}";

        try
        {
            HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"API call failed with status code: {response.StatusCode}");
                return double.MaxValue;
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseContent);

            string status = doc.RootElement.GetProperty("status").GetString();
            if (status != "OK")
            {
                Console.Error.WriteLine($"API error: {status}");
                return double.MaxValue;
            }

            var element = doc.RootElement
                .GetProperty("rows")[0]
                .GetProperty("elements")[0];

            string elementStatus = element.GetProperty("status").GetString();
            if (elementStatus != "OK")
            {
                Console.Error.WriteLine($"Element error: {elementStatus}");
                return double.MaxValue;
            }

            double distanceInMeters = element.GetProperty("distance").GetProperty("value").GetDouble();
            return distanceInMeters / 1000.0;
        }
        catch (TaskCanceledException)
        {
            Console.Error.WriteLine("Request timed out.");
            return double.MaxValue;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Network error: {ex.Message}");
            return double.MaxValue;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return double.MaxValue;
        }
    }

    //public static async Task<(double? Latitude, double? Longitude, string? Error)> GetCoordinatesFromAddressAsync(string address)
    //{
    //    string apiKey = "PK.83B935C225DF7E2F9B1ee90A6B46AD86";
    //    using var client = new HttpClient();
    //    string url = $"https://us1.locationiq.com/v1/search.php?key={apiKey}&q={Uri.EscapeDataString(address)}&format=json";

    //    try
    //    {
    //        var response = await client.GetAsync(url);
    //        if (!response.IsSuccessStatusCode)
    //            return (null, null, "Invalid address or API error.");

    //        var json = await response.Content.ReadAsStringAsync();
    //        using var doc = JsonDocument.Parse(json);

    //        if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
    //            return (null, null, "Address not found.");

    //        var root = doc.RootElement[0];

    //        double latitude = double.Parse(root.GetProperty("lat").GetString());
    //        double longitude = double.Parse(root.GetProperty("lon").GetString());

    //        return (latitude, longitude, null);
    //    }
    //    catch (Exception ex)
    //    {
    //        return (null, null, ex.Message);
    //    }
    //}
    public static async Task<(double? Latitude, double? Longitude, string? Error)> GetCoordinatesFromAddressAsync(string address)
    {
        string apiKey = "PK.83B935C225DF7E2F9B1ee90A6B46AD86";
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(7) }; 
        string url = $"https://us1.locationiq.com/v1/search.php?key={apiKey}&q={Uri.EscapeDataString(address)}&format=json";

        try
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return (null, null, "Invalid address or API error.");

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
                return (null, null, "Address not found.");

            var root = doc.RootElement[0];

            double latitude = double.Parse(root.GetProperty("lat").GetString());
            double longitude = double.Parse(root.GetProperty("lon").GetString());

            return (latitude, longitude, null);
        }
        catch (TaskCanceledException)
        {
            return (null, null, "Timeout while contacting location service.");
        }
        catch (Exception ex)
        {
            return (null, null, ex.Message);
        }
    }
    public static async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var fromAddress = new MailAddress("mailforcsharp1234@gmail.com", "NoReplyVolunteerOrganization");
        var toAddress = new MailAddress(toEmail);

        using var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("mailforcsharp1234@gmail.com", "mjhm ignt phfr whuc"),
            EnableSsl = true,
        };

        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body
        };
    }
}
