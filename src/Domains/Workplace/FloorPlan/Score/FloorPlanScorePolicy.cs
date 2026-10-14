namespace Acme.Workforce.Workplace.FloorPlan.Score;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of floor plan score policy.</summary>
public static class FloorPlanScorePolicy
{
    /// <summary>Puts a floor plan score code in its canonical FPS- form.</summary>
    public static string NormalizeFloorPlanScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFloorPlanScore()
    {
        var policy = Retry.DefaultPolicy("floor-plan-score");
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

    /// <summary>Counts how many amounts fit in a budget of 91709.0, in order.</summary>
    public static int FloorPlanScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 91709.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int FloorPlanScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }
}
