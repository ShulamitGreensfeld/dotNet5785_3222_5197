namespace Dal;
using DalApi;
using System.Reflection.Metadata.Ecma335;

internal class ConfigImplementation : IConfig
{
    public DateTime Clock { get => Config.Clock; set => Config.Clock = value; }//stage 3

    public void Reset()//stage 3
    {
        Config.Reset();
    }
    public TimeSpan RiskRange//stage 3
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }
}