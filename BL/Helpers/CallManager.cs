using DalApi;
using System;

namespace Helpers;
//לסדר את השגיאות בפונקציות
internal static class CallManager
{
    private static IDal s_dal = Factory.Get; //stage 4
    /// <summary>
    /// מקבלת מזהה קריאה ומחזירה את הסטטוס העדכני שלה לפי הנתונים בבסיס הנתונים ושעון המערכת.
    /// </summary>
    public static BO.CallStatus GetCallStatus(int callId)
    {
        var call = s_dal.Call.Read(callId);
        if (call == null) throw new ArgumentException("קריאה לא נמצאה");

        return call.CallStatus;
    }
        //  public static string GetCallStatus(int callId)
        //{
        //    var call = s_dal.Call.GetById(callId); // שימוש ב-s_dal להגעה לנתונים
        //    if (call == null)
        //        throw new ArgumentException("קריאה לא נמצאה");

        //    DateTime systemTime = ClockManager.GetCurrentTime(); // מניח ששעון המערכת מנוהל על ידי ClockManager

        //    if (call.EndTime < systemTime && call.Assignment == null)
        //    {
        //        return "פג תוקף";
        //    }

        //    if (call.Assignment != null && call.Assignment.ActualEndTime == null)
        //    {
        //        return "בהמתנה לסיום";
        //    }

        //    return "סגור";
        //}


/// <summary>
/// מעדכנת את כל הקריאות שפג תוקפן.
/// </summary>
//public static void UpdateExpiredCalls()
//    {
//        var expiredCalls = s_dal.Call.ReadAll()
//            .Where(c => c.ExpirationTime < DateTime.Now && c.CallStatus != DO.CallStatus.Closed)
//            .ToList();

//        foreach (var call in expiredCalls)
//        {
//            var assignment = s_dal.Assignment.ReadAll()
//                .FirstOrDefault(a => a.CallId == call.CallId && a.EndTime == null);

//            if (assignment == null)
//            {
//                // יצירת ישות הקצאה חדשה לקריאה שפג תוקפה
//                s_dal.Assignment.Add(new DO.Assignment
//                {
//                    CallId = call.CallId,
//                    VolunteerId = 0, // מתנדב לא מוקצה
//                    EndTime = DateTime.Now,
//                    CompletionType = DO.CompletionType.Expired
//                });
//            }
//            else
//            {
//                // עדכון הקצאה קיימת עם זמן סיום טיפול בפועל
//                assignment.EndTime = DateTime.Now;
//                assignment.CompletionType = DO.CompletionType.Expired;
//                s_dal.Assignment.Update(assignment);
//            }

//            call.Status = DO.CallStatus.Closed;
//            s_dal.Call.Update(call);
//        }
         public static void UpdateExpiredCalls()
    {
        DateTime systemTime = ClockManager.GetCurrentTime();

        foreach (var call in s_dal.Call.GetAll().Where(c => c?.EndTime < systemTime && c.Assignment == null))
        {
            // הוספת הקצאה חדשה
            s_dal.Assignment.Add(new DO.Assignment
            {
                CallId = call.Id,
                ActualEndTime = systemTime,
                CompletionStatus = "ביטול פג תוקף",
                VolunteerId = 0 // ת.ז מתנדב = 0
            });
        }

        foreach (var call in s_dal.Call.GetAll().Where(c => c?.EndTime < systemTime && c.Assignment?.ActualEndTime == null))
        {
            // עדכון הקצאה קיימת
            call.Assignment.ActualEndTime = systemTime;
            call.Assignment.CompletionStatus = "ביטול פג תוקף";
        }

        // שליחה על עדכון הקריאות
        SendCallUpdateNotifications();
    }

    private static void SendCallUpdateNotifications()
    {
        // שליחה לכל המשקיפים על עדכון הקריאה
        foreach (var call in s_dal.Call.GetAll())
        {
            // כאן תוכל לשלוח את ההודעות לכל המשקיפים
            // לדוגמה:
            ObserverManager.NotifyObservers(call);
        }

        // שליחה להודעה על עדכון כל הרשימה
        ObserverManager.NotifyAllObservers();
    }
    }
