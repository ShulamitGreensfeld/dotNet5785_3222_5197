namespace Dal;
using DalApi;
sealed internal class DalList : IDal
{
    // Lazy<IDal> ensures lazy initialization and thread safety
    private static readonly Lazy<IDal> lazyInstance = new Lazy<IDal>(() => new DalList());
    // Public static property to access the single instance
    public static IDal Instance => lazyInstance.Value;
    private DalList() { }
    public IAssignment Assignment { get; } = new AssignmentImplementation();

    public ICall Call { get; } = new CallImplementation();

    public IVolunteer Volunteer { get; } = new VolunteerImplementation();

    public IConfig Config { get; } = new ConfigImplementation();

    public void ResetDB()
    {
        Assignment.DeleteAll();
        Call.DeleteAll();
        Volunteer.DeleteAll();
        Config.Reset();
    }
}
