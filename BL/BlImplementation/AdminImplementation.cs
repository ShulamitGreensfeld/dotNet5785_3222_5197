using BlApi;
using DalApi;
using DalTest;
using Helpers;

namespace BlImplementation;

internal class AdminImplementation : IAdmin
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    public DateTime GetClock()
    {
        return ClockManager.Now;
    }
    public void PromoteClock(BO.Enums.TimeUnit timeUnit)
    {
        DateTime newClock = timeUnit switch
        {
            BO.Enums.TimeUnit.Minute => ClockManager.Now.AddMinutes(1),
            BO.Enums.TimeUnit.Hour => ClockManager.Now.AddHours(1),
            BO.Enums.TimeUnit.Day => ClockManager.Now.AddDays(1),
            BO.Enums.TimeUnit.Month => ClockManager.Now.AddMonths(1),
            BO.Enums.TimeUnit.Year => ClockManager.Now.AddYears(1),
            _ => throw new ArgumentOutOfRangeException(nameof(timeUnit), "Invalid time unit")
        };

        ClockManager.UpdateClock(newClock);
    }

    public TimeSpan GetRiskTimeRange()
    {
        return _dal.Config.RiskRange;
    }

    public void SetRiskTimeRange(TimeSpan riskTimeRange)
    {
        _dal.Config.RiskRange = riskTimeRange;
    }

    public void ResetDatabase()
    {
        _dal.Config.Reset();
        _dal.ResetDB();
        Console.WriteLine("Resetting configuration and clearing data...");
        Console.WriteLine("Resetting completed successfully!");
    }

    public void InitializeDatabase()
    {
        _dal.ResetDB();
        DalTest.Initialization.Do();
    }
}