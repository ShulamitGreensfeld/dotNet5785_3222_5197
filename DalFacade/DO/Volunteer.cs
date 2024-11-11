//Module Volunteer.cs
namespace DO;

/// <summary>
///  Volunteer Entity represents a volunteer with all its props
/// </summary>
/// <param name="ID">Personal unique ID of the Volunteer</param>
/// <param name="Name"></param>
/// <param name="Phone"></param>
/// <param name="Email"></param>
/// <param name="Password"></param>
/// <param name="Address"></param>
/// <param name="Latitude"></param>
/// <param name="Longitude"></param>
/// <param name="Role"></param>
/// <param name="IsActive"></param>
/// <param name="MaxDistanceForCall"></param>
/// <param name="DistanceType"></param>
public record Volunteer
(
   int ID, 
   string Name,
   string Phone,
   string Email,
   string? Password=null,
   string? Address = null,
   double? Latitude = null,
   double? Longitude = null,
   Role Role,
   bool IsActive,
   double? MaxDistanceForCall = null,
   DistanceType DistanceType
)
{
    /// <summary>
    /// Default constructor for stage 3
    /// </summary>
    public Volunteer();
}
