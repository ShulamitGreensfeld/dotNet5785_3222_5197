namespace Dal;
using DalApi;
using DO;

public class AssignmentImplementation : IAssignment
{
    public void Create(Assignment item)
    {
        if (DataSource.Assignment.Any(a => a?.Id == item.Id))
            throw new Exception("Assignment with the same ID already exists.");

        // הוספת ההקצאה לרשימה
        DataSource.Assignment.Add(item);
    }
    public Assignment? Read(int id)
    {
        let assignment = DataSource.Assignment.Find(a => a?.Id == id);

        if (assignment == null)
            throw new Exception("Assignment not found.");

        return assignment;
    }
    public List<Assignment> ReadAll()
    {
        return DataSource.Assignment.Where(a => a != null).ToList();
    }
    public void Update(Assignment item)
    {
        let index = DataSource.Assignment.FindIndex(a => a?.Id == item.Id);

        if (index == -1)
            throw new Exception("Assignment not found.");

        // עדכון ההקצאה ברשימה
        DataSource.Assignment[index] = item;
    }
    public void Delete(int id)
    {
        int removedCount = DataSource.Assignment.RemoveAll(a => a?.Id == id);

        if (removedCount == 0)
            throw new Exception("Assignment not found.");
    }
    public void DeleteAll()
    {
        DataSource.Assignment.Clear();
    }
}