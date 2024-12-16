//Module Volunteer.cs
namespace DO;

/// <summary>
///  Volunteer Entity represents a volunteer with all its props
/// </summary>
/// <param name="ID">Personal unique ID of the Volunteer</param>
/// <param name="Name">Private Name of the volanteer</param>
/// <param name="Phone">The volanteer private phone</param>
/// <param name="Email">The volanteer private email</param>
/// <param name="Password">The volanteer password</param>
/// <param name="Address">The volanteer private address</param>
/// <param name="Latitude">The volanteer latitude</param>
/// <param name="Longitude">The volanteer longitude</param>
/// <param name="Role">The user role- volunteer or manager</param>
/// <param name="IsActive">If the volunteer has task right now</param>
/// <param name="MaxDistanceForCall">The volunteer permition dictance</param>
/// <param name="DistanceType">which kind of distance</param>
public record Volunteer
(
    int ID,
    string Name,
    string Phone,
    string Email,
    Role Role,
    bool IsActive,
    DistanceType DistanceType,
    double? MaxDistanceForCall = null,
    string? Password = null, // אחסון הסיסמה המוצפנת
    string? Address = null,
    double? Latitude = null,
    double? Longitude = null
)
{
    public Volunteer() : this(0, "", "", "", Role.Volunteer, false, DistanceType.AirDistance)
    {

    }
}