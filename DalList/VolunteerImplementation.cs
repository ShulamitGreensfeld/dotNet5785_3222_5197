namespace Dal;
using DalApi;
using DO;

public class VolunteerImplementation : IVolunteer
{
    public void Create(Volunteer item)
    {
        if (DataSource.Volunteer.Any(v => v?.Id == item.Id))
            throw new Exception("Volunteer with the same ID already exists.");

        // הוספת המתנדב לרשימה
        DataSource.Volunteer.Add(item);
    }
    public Volunteer? Read(int id)
    {
        let volunteer = DataSource.Volunteer.Find(v => v?.Id == id);

        if (volunteer == null)
            throw new Exception("Volunteer not found.");

        return volunteer;
    }
    public List<Volunteer> ReadAll()
    {
        return DataSource.Volunteer.Where(v => v != null).ToList();
    }
    public void Update(Volunteer item)
    {
        let index = DataSource.Volunteer.FindIndex(v => v?.Id == item.Id);

        if (index == -1)
            throw new Exception("Volunteer not found.");

        // עדכון המתנדב ברשימה
        DataSource.Volunteer[index] = item;
    }
    public void Delete(int id)
    {
        int removedCount = DataSource.Volunteer.RemoveAll(v => v?.Id == id);

        if (removedCount == 0)
            throw new Exception("Volunteer not found.");
    }
    public void DeleteAll()
    {
        DataSource.Volunteer.Clear();
    }

}
