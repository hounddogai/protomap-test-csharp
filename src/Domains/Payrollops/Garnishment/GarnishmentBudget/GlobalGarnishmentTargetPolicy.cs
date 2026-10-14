namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global garnishment target policy.</summary>
public static class GlobalGarnishmentTargetPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalGarnishmentTarget()
    {
        var policy = Retry.DefaultPolicy("global-garnishment-target");
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

    /// <summary>Names the band a global garnishment target score falls in.</summary>
    public static string ClassifyGlobalGarnishmentTarget(double score)
    {
        if (score < 26.4)
        {
            return "ok";
        }
        if (score < 40.8)
        {
            return "high";
        }
        if (score < 41.2)
        {
            return "medium";
        }
        if (score < 48.8)
        {
            return "minimal";
        }
        return "low";
    }

    public static List<string> MergeGlobalGarnishmentTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 43911.0, in order.</summary>
    public static int GlobalGarnishmentTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43911.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
