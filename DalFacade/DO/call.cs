//Module Volunteer.cs
namespace DO;

/// <summary>
/// Call Entity represents a call with all its props
/// </summary>
/// <param name="ID"></param>
/// <param name="TypeOfCall"></param>
/// <param name="Address"></param>
/// <param name="Latitude"></param>
/// <param name="Longitude"></param>
/// <param name="OpeningTime"></param>
/// <param name="MaxTimeForClosing"></param>
/// <param name="CallDescription"></param>
public record Call
(
   int ID,
   TypeOfCall TypeOfCall,
   string Address,
   double Latitude,
   double Longitude,
   DateTime OpeningTime,
   DateTime? MaxTimeForClosing=null,
   string? CallDescription = null
)
{
    /// <summary>
    /// Default constructor for stage 3
    /// </summary>
    public Call() : this(0, TypeOfCall.ToCarryFood, "", 0, 0, DateTime.Now) { }
}