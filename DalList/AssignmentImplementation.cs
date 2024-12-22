namespace Dal;
using DalApi;
using DO;
using System.Linq;

internal class AssignmentImplementation : IAssignment
{
	public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null) //stage 2
	{ 
		return filter != null
			? from item in DataSource.Assignments
			  where filter(item)
			  select item
			: from item in DataSource.Assignments
			  select item;
	}
	public void Create(Assignment item)
	{
		Assignment copy = item with { ID = Config.NextAssignmentId };
		DataSource.Assignments.Add(copy);
	}

    public void Delete(int id)
	{
		Assignment? newAssignment = Read(id);
		if (newAssignment == null)
			throw new DalDeletionImpossibleException($"Assignment with ID={id} does Not exist");
		else
			DataSource.Assignments.Remove(newAssignment);
	}

	public void DeleteAll()
	{
		DataSource.Assignments.Clear();
	}

	public Assignment? Read(int id)
	{
		//Assignment? newAssignment = DataSource.Assignments.Find(assignment => assignment!.ID == id); //stage1
		//return newAssignment; //stage1
		Assignment? newAssignment = DataSource.Assignments.FirstOrDefault(assignment => assignment!.ID == id); //stage 2
		return newAssignment; //stage2
	}

	//   public List<Assignment> ReadAll() //stage1
	//{
	//	return new List<Assignment>(DataSource.Assignments!);
	//}

	public void Update(Assignment item)
	{
        Assignment? newAssignment = Read(item.ID);
        if (newAssignment == null)
			throw new DalDoesNotExistException($"Assignment with ID={item.ID} does Not exist");
		else
		{
			DataSource.Assignments.Remove(newAssignment);
			DataSource.Assignments.Add(item);
		}
	}

    public Assignment? Read(Func<Assignment, bool> filter)
    {
        if (filter == null)
            throw new NullException($"{nameof(filter)} Filter function cannot be null");

        return DataSource.Assignments.Cast<Assignment>().FirstOrDefault(filter);
    }
}
