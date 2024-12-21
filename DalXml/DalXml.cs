using DalApi;
namespace Dal;

sealed public class DalXml : IDal
{
    public ICall Call { get; } = new CallImplementation();  //stage 3
    public IVolunteer Volunteer { get; } = new VolunteerImplementation(); //stage 3
    public IAssignment Assignment { get; } = new AssignmentImplementation();//stage 3
    public IConfig Config { get; } = new ConfigImplementation();//stage 3

    public void ResetDB()//stage 3
    {
        Call.DeleteAll();
        Volunteer.DeleteAll();
        Assignment.DeleteAll();
        Config.Reset();
    }

}