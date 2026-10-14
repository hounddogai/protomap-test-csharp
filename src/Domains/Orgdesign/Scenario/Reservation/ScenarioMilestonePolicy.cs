namespace Acme.Workforce.Orgdesign.Scenario.Reservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scenario milestone policy.</summary>
public static class ScenarioMilestonePolicy
{
    /// <summary>Names the band a scenario milestone score falls in.</summary>
    public static string ClassifyScenarioMilestone(double score)
    {
        if (score < 2.5)
        {
            return "high";
        }
        if (score < 3.8)
        {
            return "low";
        }
        if (score < 12.4)
        {
            return "ok";
        }
        if (score < 40.8)
        {
            return "elevated";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 90244.0, in order.</summary>
    public static int ScenarioMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90244.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScenarioMilestone()
    {
        var policy = Retry.DefaultPolicy("scenario-milestone");
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

    public static string ScenarioMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
