using BlApi;
using DalTest;
using Helpers;

namespace BlImplementation
{
    internal class AdminImplementation : IAdmin
    {
        private readonly DalApi.IDal Admin_dal = DalApi.Factory.Get;

        // מתודת בקשת שעון
        public DateTime Clock()
        {
            // מחזיר את הזמן הנוכחי מהשעון המערכת
            return ClockManager.Now;
        }

        // מתודת קידום שעון
        public void PromotionClock(BO.TimeUnit timeUnit)
        {
            //DateTime newTime;
            // קידום השעון בהתאם ליחידת הזמן
            switch (timeUnit)
            {
                case BO.TimeUnit.MINUTE:
                    ClockManager.UpdateClock(ClockManager.Now.AddMinutes(1));
                    break;
                case BO.TimeUnit.HOUR:
                    ClockManager.UpdateClock(ClockManager.Now.AddHours(1));
                    break;
                case BO.TimeUnit.DAY:
                    ClockManager.UpdateClock(ClockManager.Now.AddDays(1));
                    break;
                case BO.TimeUnit.MONTH:
                    ClockManager.UpdateClock(ClockManager.Now.AddMonths(1));
                    break;
                case BO.TimeUnit.YEAR:
                    ClockManager.UpdateClock(ClockManager.Now.AddYears(1));
                    break;
                default:
                    throw new ArgumentException("Unknown time unit");
            }
        }

        // מתודת בקשת טווח זמן סיכון
        public TimeSpan GetRiskRange()
        {
            // מחזיר את טווח הזמן של הסיכון שנשמר בתצורה
            return Admin_dal.Config.RiskRange;
        }

        // מתודת הגדרת טווח זמן סיכון
        public void SetRiskRange(TimeSpan riskRange)
        {
            // מעדכן את טווח הזמן של הסיכון בתצורה
            Admin_dal.Config.RiskRange = riskRange;
        }

        // מתודת איפוס בסיס נתונים
        public void ResetDB()
        {
            Admin_dal.Config.Reset();
            Admin_dal.Volunteer.DeleteAll();
            Admin_dal.Assignment.DeleteAll();
            Admin_dal.Call.DeleteAll();
        }

        // מתודת אתחול בסיס נתונים
        public void InitializeDB()
        {
            // אתחול של בסיס הנתונים: איפוס וטעינה מחדש של הנתונים
            ResetDB();
            // אתחול הישויות עם ערכים התחלתיים
            Initialization.Do();
        }
    }
}