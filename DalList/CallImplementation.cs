namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

internal class CallImplementation : ICall
{
    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null) //stage 2
    { 
        return filter != null
            ? from item in DataSource.Calls
              where filter(item)
              select item
            : from item in DataSource.Calls
              select item;
    }
    public void Create(Call item)
    {
        Call copy = item with { ID = Config.NextCallId };
        DataSource.Calls.Add(copy);
    }

    public void Delete(int id)
    {
        Call? newCall = Read(id);
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
        //Call? newCall = DataSource.Calls.Find(call => call!.ID == id); //stage1
        //return newCall; //stage1
        Call? newCall = DataSource.Calls.FirstOrDefault(call => call!.ID == id); //stage 2
        return newCall; //stage2
    }

    //public List<Call> ReadAll() //stage1
    //{
    //    return new List<Call>(DataSource.Calls!);
    //}
    public void Update(Call item)
    {
        Call? existingCall = Read(item.ID);
        if (existingCall == null)
            throw new Exception($"Call with ID={item.ID} does Not exist");
        else
        {
            Delete(item.ID); 
            Create(item); 
                          
        }
    }
}