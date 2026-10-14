namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scenario segment policy.</summary>
public static class ScenarioSegmentPolicy
{
    /// <summary>Names the band a scenario segment score falls in.</summary>
    public static string ClassifyScenarioSegment(double score)
    {
        if (score < 78.9)
        {
            return "medium";
        }
        if (score < 80.4)
        {
            return "low";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 65883.0, in order.</summary>
    public static int ScenarioSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 65883.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScenarioSegment()
    {
        var policy = Retry.DefaultPolicy("scenario-segment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string ScenarioSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
