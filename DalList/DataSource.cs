 namespace Dal;
internal static class DataSource
{
    internal static List<DO.Volunteer?> Volunteers { get; } = new List<DO.Volunteer?>();
    internal static List<DO.Call?> Calls { get; } = new List<DO.Call?>();
    internal static List<DO.Assignment?> Assignments { get; } = new List<DO.Assignment?>();
}