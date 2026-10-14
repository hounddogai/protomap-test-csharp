namespace Acme.Workforce.Orgdesign.Scenario.Quota;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted scenario override policy.</summary>
public static class WeightedScenarioOverridePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedScenarioOverride()
    {
        var policy = Retry.DefaultPolicy("weighted-scenario-override");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string WeightedScenarioOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double WeightedScenarioOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a weighted scenario override score falls in.</summary>
    public static string ClassifyWeightedScenarioOverride(double score)
    {
        if (score < 7.7)
        {
            return "watch";
        }
        if (score < 92.2)
        {
            return "ok";
        }
        return "low";
    }
}
