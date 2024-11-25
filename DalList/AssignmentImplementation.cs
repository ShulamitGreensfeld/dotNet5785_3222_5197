namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class AssignmentImplementation : IAssignment
{
    public void Create(Assignment item)
    {
        Assignment copy = item with { ID = Config.NextAssignmentId };//hkhkj
        DataSource.Assignments.Add(copy);
    }

    public void Delete(int id)
    {
        Assignment? newAssignment = Read(id);
        if (newAssignment == null)
            throw new Exception($"Assignment with ID={id} does Not exist");
        else
            DataSource.Assignments.Remove(newAssignment);
    }

    public void DeleteAll()
    {
        DataSource.Assignments.Clear();
    }

    public Assignment? Read(int id)
    {
        Assignment? newAssignment = DataSource.Assignments.Find(assignment => assignment!.ID == id);
        return newAssignment;
    }

    public List<Assignment> ReadAll()
    {
        return new List<Assignment>(DataSource.Assignments!);
    }

    public void Update(Assignment item)
    {
        Assignment? newAssignment = DataSource.Assignments.Find(assignment => assignment!.ID == item.ID);
        if (newAssignment == null)
            throw new Exception($"Assignment with ID={item.ID} does Not exist");
        else
        {
            DataSource.Assignments.Remove(newAssignment);
            DataSource.Assignments.Add(item);
        }
    }
}
