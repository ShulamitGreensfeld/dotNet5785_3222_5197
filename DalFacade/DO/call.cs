namespace DO;

/// <summary>
/// Call Entity represents a call with all its props
/// </summary>
/// <param name="ID">Unique identifier for the call.</param>
/// <param name="TypeOfCall">Type of the call (e.g., carrying food, emergency, etc.).</param>
/// <param name="Address">The address where the call takes place.</param>
/// <param name="Latitude">Latitude of the call location.</param>
/// <param name="Longitude">Longitude of the call location.</param>
/// <param name="OpeningTime">The time when the call is opened.</param>
/// <param name="MaxTimeForClosing">Optional max time for closing the call.</param>
/// <param name="CallDescription">Optional description of the call.</param>
public record Call
(
   TypeOfCall TypeOfCall,
   string Address,
   double Latitude,
   double Longitude,
   DateTime OpeningTime,
   TimeSpan? RiskRange= null,
   DateTime? MaxTimeForClosing=null,
   string? CallDescription = null
)
{
    public int ID { get; init; }
    /// <summary>
    /// Default constructor for stage 3
    /// </summary>
    public Call() : this( TypeOfCall.ToCarryFood, "", 0, 0, DateTime.Now) { }
}