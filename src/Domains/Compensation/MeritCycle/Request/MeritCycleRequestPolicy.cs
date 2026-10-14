namespace Acme.Workforce.Compensation.MeritCycle.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of merit cycle request policy.</summary>
public static class MeritCycleRequestPolicy
{
    /// <summary>Names the band a merit cycle request score falls in.</summary>
    public static string ClassifyMeritCycleRequest(double score)
    {
        if (score < 12.6)
        {
            return "low";
        }
        if (score < 14.3)
        {
            return "high";
        }
        if (score < 22.0)
        {
            return "watch";
        }
        return "elevated";
    }

    public static string MeritCycleRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
