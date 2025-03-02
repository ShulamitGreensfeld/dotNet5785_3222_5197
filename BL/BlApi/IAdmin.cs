namespace BlApi
{
    public interface IAdmin
    {
        DateTime GetClock();
        void PromoteClock(BO.Enums.TimeUnit timeUnit);
        TimeSpan GetRiskTimeRange();
        void SetRiskTimeRange(TimeSpan riskTimeRange);
        void ResetDatabase();
        void InitializeDatabase();
    }

}