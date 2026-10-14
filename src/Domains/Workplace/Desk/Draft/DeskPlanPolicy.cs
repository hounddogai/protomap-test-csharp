namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of desk plan policy.</summary>
public static class DeskPlanPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeskPlan()
    {
        var policy = Retry.DefaultPolicy("desk-plan");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double DeskPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a desk plan score falls in.</summary>
    public static string ClassifyDeskPlan(double score)
    {
        if (score < 14.6)
        {
            return "high";
        }
        if (score < 20.2)
        {
            return "watch";
        }
        if (score < 52.8)
        {
            return "medium";
        }
        return "minimal";
    }

    /// <summary>Puts a desk plan code in its canonical DP- form.</summary>
    public static string NormalizeDeskPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
