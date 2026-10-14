namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal scenario request policy.</summary>
public static class SeasonalScenarioRequestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalScenarioRequest()
    {
        var policy = Retry.DefaultPolicy("seasonal-scenario-request");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double SeasonalScenarioRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.47;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a seasonal scenario request score falls in.</summary>
    public static string ClassifySeasonalScenarioRequest(double score)
    {
        if (score < 30.4)
        {
            return "ok";
        }
        if (score < 70.4)
        {
            return "watch";
        }
        return "critical";
    }

    public static string SeasonalScenarioRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
