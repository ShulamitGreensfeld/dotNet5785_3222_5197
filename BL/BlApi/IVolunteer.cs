namespace BlApi
{
    public interface IVolunteer
    {
        BO.Enums.Role EnterSystem(string name, string pass);
        IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActive = null, BO.Enums.VolunteerInListFields? fieldFilter = null);
        BO.Volunteer GetVolunteerDetails(int id);
        void UpdateVolunteerDetails(int id, BO.Volunteer volunteer);
        void DeleteVolunteer(int id);
        void AddVolunteer(BO.Volunteer volunteer);
    }
}