using BlApi;
using Helpers;
using static BO.Enums;

namespace BlImplementation;

internal class AdminImplementation : IAdmin
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    /// <summary>
    /// Retrieves the current system clock time.
    /// </summary>
    public DateTime GetClock() => AdminManager.Now;

    /// <summary>
    /// Advances the system clock by a specified time unit.
    /// </summary>
    public void PromoteClock(TimeUnit timeUnit)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        DateTime oldClock = GetClock();
        DateTime newClock = oldClock;

        switch (timeUnit)
        {
            case TimeUnit.Minute:
                newClock = oldClock.AddMinutes(1); break;
            case TimeUnit.Hour:
                newClock = oldClock.AddHours(1); break;
            case TimeUnit.Day:
                newClock = oldClock.AddDays(1); break;
            case TimeUnit.Month:
                newClock = oldClock.AddMonths(1); break;
            case TimeUnit.Year:
                newClock = oldClock.AddYears(1); break;
        }

        AdminManager.UpdateClock(newClock);
    }

    /// <summary>
    /// Retrieves the current risk time range.
    /// </summary>
    public TimeSpan GetRiskTimeRange() => AdminManager.RiskRange;

    /// <summary>
    /// Sets a new risk time range.
    /// </summary>
    public void SetRiskTimeRange(TimeSpan RiskRange)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        AdminManager.RiskRange = RiskRange;
    }

    /// <summary>
    /// Resets the system's database and configuration.
    /// </summary>
    public void ResetDatabase()
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        AdminManager.ResetDB();
        _dal.ResetDB();

        Console.WriteLine("Resetting configuration and clearing data...");
        Console.WriteLine("Resetting completed successfully!");
    }

    /// <summary>
    /// Initializes the system's database with test data.
    /// </summary>
    public void InitializeDatabase()
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        AdminManager.InitializeDB();
        _dal.ResetDB();
        DalTest.Initialization.Do();
        VolunteerManager.InvokeCallsListUpdated();
        CallManager.Observers.NotifyListUpdated();
    }

    #region Stage 5 – Event Observers
    public void AddClockObserver(Action clockObserver) =>
        AdminManager.ClockUpdatedObservers += clockObserver;

    public void RemoveClockObserver(Action clockObserver) =>
        AdminManager.ClockUpdatedObservers -= clockObserver;

    public void AddConfigObserver(Action configObserver) =>
        AdminManager.ConfigUpdatedObservers += configObserver;

    public void RemoveConfigObserver(Action configObserver) =>
        AdminManager.ConfigUpdatedObservers -= configObserver;
    #endregion
    public void StartSimulator(int interval)  //stage 7
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        AdminManager.Start(interval); //stage 7
    }
     public void StopSimulator()
      => AdminManager.Stop(); //stage 7

}