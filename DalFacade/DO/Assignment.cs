//Module Volunteer.cs
namespace DO;

/// <summary>
///  Assignment Entity represents a Assignment with all its props
/// </summary>
/// <param name="ID"></param>
/// <param name="CallId"></param>
/// <param name="VolunteerId"></param>
/// <param name="EntryTimeForTreatment"></param>
/// <param name="EndTimeForTreatment"></param>
/// <param name="TypeOfFinishTreatment"></param>
public record Assignment
(
   int ID,
   int CallId,
   int VolunteerId,
   DateTime EntryTimeForTreatment,
   DateTime? EndTimeForTreatment=null,
   TypeOfFinishTreatment? TypeOfFinishTreatment=null
)
{
    /// <summary>
    /// Default constructor for stage 3
    /// </summary>
    public Assignment() : this(0, 0, 0, DateTime.Now) { }
}