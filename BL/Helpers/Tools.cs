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
    public static double CalculateDistance(object latitude1, object longitude1, double latitude2, double longitude2)
    {
        // המרת פרמטרים מסוג object ל-double
        if (!double.TryParse(latitude1?.ToString(), out double lat1) ||
            !double.TryParse(longitude1?.ToString(), out double lon1))
        {
            throw new ArgumentException("Invalid latitude or longitude values.");
        }

        const double EarthRadiusKm = 6371; // רדיוס כדור הארץ בקילומטרים

        // המרת מעלות לרדיאנים
        double lat1Rad = DegreesToRadians(lat1);
        double lon1Rad = DegreesToRadians(lon1);
        double lat2Rad = DegreesToRadians(latitude2);
        double lon2Rad = DegreesToRadians(longitude2);

        // חישוב ההפרשים
        double deltaLat = lat2Rad - lat1Rad;
        double deltaLon = lon2Rad - lon1Rad;

        // נוסחת Haversine
        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // חישוב המרחק
        double distance = EarthRadiusKm * c;
        return distance;
    }
    public static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
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

        // המרת הערכים ל-double
        double latitude = double.Parse(root.GetProperty("lat").GetString());
        double longitude = double.Parse(root.GetProperty("lon").GetString());

        return (latitude, longitude);
    }
    //public static void SendEmail(string toEmail, string subject, string body)
    //{
    //    var fromAddress = new MailAddress("projectydidim@gmail.com", "Ydidim");
    //    var toAddress = new MailAddress(toEmail);

    //    var smtpClient = new SmtpClient("smtp.gmail.com")
    //    {
    //        Port = 587,
    //        Credentials = new NetworkCredential("yedidim.tzippi.mali@gmail.com", "zdjq kchm vqqi nure"),
    //        EnableSsl = true,
    //    };

    //    using (var message = new MailMessage(fromAddress, toAddress)
    //    {
    //        Subject = subject,
    //        Body = body,
    //    })
    //    {
    //        smtpClient.Send(message);
    //    }
    //}
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

    public static void StorePasswordInXml(string volunteerName, string hashedPassword, string filePath)   //לבדוק אם צריך את הפונקציה הזאת כי אל נעשה בה שימוש ככל הנראה...
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