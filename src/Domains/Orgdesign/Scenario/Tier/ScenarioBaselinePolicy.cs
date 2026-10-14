namespace Acme.Workforce.Orgdesign.Scenario.Tier;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scenario baseline policy.</summary>
public static class ScenarioBaselinePolicy
{
    /// <summary>Names the band a scenario baseline score falls in.</summary>
    public static string ClassifyScenarioBaseline(double score)
    {
        if (score < 21.4)
        {
            return "minimal";
        }
        if (score < 36.9)
        {
            return "ok";
        }
        if (score < 42.4)
        {
            return "high";
        }
        if (score < 98.1)
        {
            return "elevated";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScenarioBaseline()
    {
        var policy = Retry.DefaultPolicy("scenario-baseline");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double ScenarioBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
