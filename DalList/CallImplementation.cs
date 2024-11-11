namespace Dal;
using DalApi;
using DO;

public class CallImplementation : ICall
{
    public void Create(Call item)
    {
        if (DataSource.Call.Any(c => c?.Id == item.Id))
            throw new Exception("Call with the same ID already exists.");

        // הוספת הקריאה לרשימה
        DataSource.Call.Add(item);
    }
    public Call? Read(int id)
    {
        let call = DataSource.Call.Find(c => c?.Id == id);

        if (call == null)
            throw new Exception("Call not found.");

        return call;
    }
    public List<Call> ReadAll()
    {
        return DataSource.Call.Where(c => c != null).ToList();
    }
    public void Update(Call item)
    {
        let index = DataSource.Call.FindIndex(c => c?.Id == item.Id);

        if (index == -1)
            throw new Exception("Call not found.");

        // עדכון הקריאה ברשימה
        DataSource.Call[index] = item;
    }
    public void Delete(int id)
    {
        int removedCount = DataSource.Call.RemoveAll(c => c?.Id == id);

        if (removedCount == 0)
            throw new Exception("Call not found.");
    }
    public void DeleteAll()
    {
        DataSource.Call.Clear();
    }
}