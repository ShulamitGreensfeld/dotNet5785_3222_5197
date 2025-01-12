//namespace BlApi;
//public interface IAdmin
//{
//    DateTime Clock();
//    void PromotionClock(TimeUnit timeUnit);

//    TimeSpan GetRiskRange();
//    void SetRiskRange(TimeSpan riskRange);
//    void ResetDB();
//    void InitializeDB();
//}


namespace BlApi;

public interface IAdmin
{
    void InitializeDB();
    void ResetDB();
    int GetMaxRange();
    void SetMaxRange(int maxRange);
    DateTime GetClock();
    void AdvanceClock(BO.TimeUnit unit);
}
