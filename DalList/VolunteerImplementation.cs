namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class VolunteerImplementation : IVolunteer
{
    public void Create(Volunteer item)
    {
        if (DataSource.Volunteers.Any(v => v?.ID == item.ID))
            throw new Exception($"Volunteer with ID={item.ID} is already exist");
        DataSource.Volunteers.Add(item);
    }

    public void Delete(int id)
    {

        Volunteer? newVolunteer = DataSource.Volunteers.Find(volunteer => volunteer.ID == id);
        if (newVolunteer == null)
            throw new Exception($"Volunteer with ID={id} does Not exist");
        else
            DataSource.Volunteers.Remove(newVolunteer);
    }

    public void DeleteAll()
    {
        DataSource.Volunteers.Clear();
    }

    public Volunteer? Read(int id)
    {
        Volunteer? newvolunteer = DataSource.Volunteers.Find(volunteer => volunteer.ID == id);
        return newvolunteer;
    }

    public List<Volunteer> ReadAll()
    {
        return new List<Volunteer>(DataSource.Volunteers);
    }

    public void Update(Volunteer item)
    {
        Volunteer? newVolunteer = DataSource.Volunteers.Find(volunteer => volunteer.ID == item.ID);
        if (newVolunteer == null)
            throw new Exception($"Volunteer with ID={item.ID} does Not exist");
        else
        {
            DataSource.Volunteers.Remove(newVolunteer);
            DataSource.Volunteers.Add(item);
        }
    }
}
