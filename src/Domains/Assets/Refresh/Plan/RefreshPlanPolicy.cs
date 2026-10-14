namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of refresh plan policy.</summary>
public static class RefreshPlanPolicy
{
    /// <summary>Names the band a refresh plan score falls in.</summary>
    public static string ClassifyRefreshPlan(double score)
    {
        if (score < 23.1)
        {
            return "low";
        }
        if (score < 38.7)
        {
            return "high";
        }
        if (score < 71.1)
        {
            return "elevated";
        }
        if (score < 99.5)
        {
            return "watch";
        }
        return "critical";
    }

    public static string RefreshPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
