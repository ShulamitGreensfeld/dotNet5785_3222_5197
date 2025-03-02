using BlApi;
using BO;
using Helpers;
using static BO.Enums;

namespace BlImplementation;

internal class AdminImplementation : IAdmin
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    public DateTime GetClock()
    {
        return ClockManager.Now;
    }

    public TimeSpan GetRiskTimeRange()
    {
        return _dal.Config.RiskRange;
    }

    public void InitializeDatabase()
    {
        _dal.ResetDB();
        DalTest.Initialization.Do();
    }

    public void PromoteClock(BO.Enums.TimeUnit timeUnit)
    {
        DateTime newClock = timeUnit switch
        {
            TimeUnit.Minute => ClockManager.Now.AddMinutes(1),
            TimeUnit.Hour => ClockManager.Now.AddHours(1),
            TimeUnit.Day => ClockManager.Now.AddDays(1),
            TimeUnit.Month => ClockManager.Now.AddMonths(1),
            TimeUnit.Year => ClockManager.Now.AddYears(1),
            _ => throw new ArgumentOutOfRangeException(nameof(timeUnit), "Invalid time unit")
        };

        ClockManager.UpdateClock(newClock);
    }

    public void ResetDatabase()
    {
        _dal.Config.Reset();
        _dal.ResetDB();
    }

    public void SetRiskTimeRange(TimeSpan riskTimeRange)
    {
        _dal.Config.RiskRange = riskTimeRange;
    }
}