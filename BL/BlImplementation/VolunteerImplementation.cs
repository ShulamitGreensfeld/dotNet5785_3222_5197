namespace BlImplementation;
using BlApi;
using BO;

/*internal class VolunteerImplementation : IVolunteer
{
    public Role Login(string name, string password)
    {
        Volunteer volunteer = DO.Volunteer.FirstOrDefault(v => v.name == name);

        if (volunteer == null)
        {
            throw new InvalidOperationException("משתמש לא קיים");
        }

        if (volunteer.Password != password)
        {
            throw new InvalidOperationException("סיסמה לא נכונה");
        }

        return volunteer.MyRole;
    }
}*/