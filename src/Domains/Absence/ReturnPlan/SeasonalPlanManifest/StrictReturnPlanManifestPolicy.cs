namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict return plan manifest policy.</summary>
public static class StrictReturnPlanManifestPolicy
{
    public static double StrictReturnPlanManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 53686.0, in order.</summary>
    public static int StrictReturnPlanManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53686.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictReturnPlanManifest()
    {
        var policy = Retry.DefaultPolicy("strict-return-plan-manifest");
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

    /// <summary>Names the band a strict return plan manifest score falls in.</summary>
    public static string ClassifyStrictReturnPlanManifest(double score)
    {
        if (score < 5.1)
        {
            return "medium";
        }
        if (score < 47.9)
        {
            return "ok";
        }
        if (score < 73.1)
        {
            return "low";
        }
        if (score < 74.9)
        {
            return "critical";
        }
        return "high";
    }
}
