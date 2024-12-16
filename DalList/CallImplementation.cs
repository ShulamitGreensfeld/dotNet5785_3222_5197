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
    //public void Create(Call item)
    //{
    //    Call copy = item with { ID = Config.NextCallId };
    //    DataSource.Calls.Add(copy);
    //}
    public int Create(Call item)
    {
        // בדיקה אם קיים כבר אובייקט עם אותו מזהה
        Call? existingCall = DataSource.Calls.Find(element => element!.ID == item.ID);
        if (existingCall != null)
            throw new DalAlreadyExistsException($"An object of type Call with this ID {item.ID} already exists");
        int newId = Dal.Config.NextCallId;
        Call callCopy = item with { ID = newId };
        DataSource.Calls.Add(callCopy);
        return callCopy.ID;
    }


    public void Delete(int id)
    {
        Call? newCall = Read(id);
        if (newCall == null)
            throw new DalDeletionImpossibleException($"Call with ID={id} does Not exist");
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
            throw new DalDoesNotExistException($"Call with ID={item.ID} does Not exist");
        else
        {
            Delete(item.ID);
            Create(item);

        }
    }
    public Call? Read(Func<Call, bool> filter)
    {
        if (filter == null)
            throw new NullException($"{nameof(filter)} Filter function cannot be null");

        return DataSource.Calls.Cast<Call>().FirstOrDefault(filter);
    }

    void ICrud<Call>.Create(Call item)
    {
        throw new NotImplementedException();
    }
}