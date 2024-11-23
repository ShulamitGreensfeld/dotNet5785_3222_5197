namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class CallImplementation : ICall
{
    public void Create(Call item)
    {
        int newId = Config.NextCallId;
        Call copy = item with { ID = newId};
        DataSource.Calls.Add(copy);
    }

    public void Delete(int id)
    {
        Call? newCall = DataSource.Calls.Find(call => call.ID == id);
        if (newCall == null)
            throw new Exception($"Call with ID={id} does Not exist");
        else
            DataSource.Calls.Remove(newCall);
    }

    public void DeleteAll()
    {
        DataSource.Calls.Clear();
    }

    public Call? Read(int id)
    {
        Call? newCall = DataSource.Calls.Find(call => call.ID == id);
        return newCall;
    }

    public List<Call> ReadAll()
    {
        return new List<Call>(DataSource.Calls);
    }

    public void Update(Call item)
    {
        Call? newCall = DataSource.Calls.Find(call => call.ID == item.ID);
        if (newCall == null)
            throw new Exception($"Call with ID={item.ID} does Not exist");
        else
        {
            DataSource.Calls.Remove(newCall);
            DataSource.Calls.Add(item);
        }
    }
}
