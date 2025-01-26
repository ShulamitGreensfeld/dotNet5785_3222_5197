using BO;

namespace BlApi;

public interface IAdmin
{
    DateTime Clock();
    void PromotionClock(TimeUnit timeUnit);
    TimeSpan GetRiskRange();
    void SetRiskRange(TimeSpan riskRange);
    void ResetDB();
    void InitializeDB();
}