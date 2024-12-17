namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;

internal class CallImplementation : ICall
{
    //public void Create(Call item)
    //{
    //    //בדיקה אם קיים כבר אובייקט עם אותו מזהה
    //    int newId = Config.NextCallId;
    //    List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
    //    // בודקים אם יש אובייקט עם אותו ID
    //    Call? existingCall = Calls.Find(element => element.ID == newId);
    //    if (existingCall != null)
    //        throw new DalAlreadyExistsException($"An object of type Call with this ID {newId} already exists");

    //    Call callCopy = item with { ID = newId };
    //    Calls.Add(callCopy);
    //    XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml); // נשמור את הרשימה המעודכנת

    //}
    public void Create(Call item)
    {
        // שלב 1: שליפת ה-ID הבא מה-XML
        int newId = XMLTools.GetAndIncreaseConfigIntVal("data-config.xml", "nextCallId");

        // שלב 2: הגדרת ה-ID לאובייקט החדש
        Call callCopy = item with { ID = newId };

        // שלב 3: הוספת אובייקט הקריאה לרשימה
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        Calls.Add(callCopy);

        // שלב 4: שמירת הרשימה המעודכנת לקובץ XML
        XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml);
    }


    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);

        if (filter != null)
            return Calls.Where(filter);
        else
            return Calls;
    }

    public void Delete(int id)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        Call? existingCall = Read(id);
        if (existingCall == null)
            throw new DalDeletionImpossibleException($"Call with ID={id} does not exist");

        Calls.Remove(existingCall);  // מסירים את הקריאה
        XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml); // נשמור את הרשימה המעודכנת
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Call>(), Config.s_calls_xml); // נמחק את כל הקריאות
    }

    public Call? Read(int id)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        return Calls.FirstOrDefault(call => call.ID == id);
    }

    public void Update(Call item)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        Call? existingCall = Read(item.ID);
        if (existingCall == null)
            throw new DalDoesNotExistException($"Call with ID={item.ID} does not exist");

        // מסירים את הקריאה הישנה ומוסיפים את החדשה
        Calls.Remove(existingCall);
        Calls.Add(item);
        XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml); // נשמור את הרשימה המעודכנת
    }

    // קריאה לפי פונקציית סינון
    public Call? Read(Func<Call, bool> filter)
    {
        if (filter == null)
            throw new NullException($"{nameof(filter)} function cannot be null");

        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        return Calls.FirstOrDefault(filter);
    }
}
