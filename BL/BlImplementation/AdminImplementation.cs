
using DalApi;
using Helpers;

namespace BlImplementation;

internal class AdminImplementation
{
    private static readonly IDal _dal = Factory.Get;
    public DateTime GetClock()
    {
        return ClockManager.Now;
    }
    public void AdvanceClock(TimeUnit unit)
    {
        DateTime newClock = unit switch
        {
            TimeUnit.MINUTE => ClockManager.Now.AddMinutes(1),
            TimeUnit.HOUR => ClockManager.Now.AddHours(1),
            TimeUnit.DAY => ClockManager.Now.AddDays(1),
            TimeUnit.MONTH => ClockManager.Now.AddMonths(1),
            TimeUnit.YEAR => ClockManager.Now.AddYears(1),
            _ => throw new ArgumentOutOfRangeException(nameof(unit), "Invalid time unit")
        };

        ClockManager.UpdateClock(newClock);
    }

    public TimeSpan GetRiskTimeSpan()
    {
        return _dal.Config.RiskRange;
    }

    public void SetRiskTimeSpan(TimeSpan riskTimeSpan)
    {
        _dal.Config.RiskRange = riskTimeSpan;
    }

    public void ResetDatabase()
    {
        _dal.Config.ResetToDefaults();
        _dal.ClearAllEntities();
    }

    public void InitializeDatabase()
    {
        ResetDatabase();
        _dal.Initialization(); // הוספת נתוני התחלה
    }
}