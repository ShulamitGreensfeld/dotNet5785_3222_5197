namespace DO;

/// <summary>
/// Assignment Entity represents an Assignment with all its properties.
/// </summary>
/// <param name="ID">The unique identifier for the assignment.</param>
/// <param name="CallId">The unique identifier for the associated call.</param>
/// <param name="VolunteerId">The unique identifier for the volunteer assigned to the treatment.</param>
/// <param name="EntryTimeForTreatment">The time when the treatment begins.</param>
/// <param name="EndTimeForTreatment">The time when the treatment ends (optional, null by default).</param>
/// <param name="TypeOfFinishTreatment">The type of finish for the treatment (optional, null by default).</param>
public record Assignment
(
   int CallId,
   int VolunteerId,
   DateTime EntryTimeForTreatment,
   DateTime? EndTimeForTreatment=null,
   TypeOfFinishTreatment? TypeOfFinishTreatment=null
)
{
    public int ID { get; set; }
    /// <summary>
    /// Default constructor for stage 3
    /// </summary>
    public Assignment() : this(0, 0, DateTime.Now) { }
}