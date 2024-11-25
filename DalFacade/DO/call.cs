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
   TypeOfCall TypeOfCall,
   string Address,
   double Latitude,
   double Longitude,
   DateTime OpeningTime,
   DateTime ClosingTime,
   DateTime? MaxTimeForClosing=null,
   string? CallDescription = null
)
{
    public int ID { get; init; }
    /// <summary>
    /// Default constructor for stage 3
    /// </summary>
    public Call() : this( TypeOfCall.ToCarryFood, "", 0, 0, DateTime.Now, DateTime.Now) { }
}