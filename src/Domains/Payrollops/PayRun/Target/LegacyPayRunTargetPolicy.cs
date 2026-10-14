namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy pay run target policy.</summary>
public static class LegacyPayRunTargetPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyPayRunTarget()
    {
        var policy = Retry.DefaultPolicy("legacy-pay-run-target");
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

    /// <summary>Names the band a legacy pay run target score falls in.</summary>
    public static string ClassifyLegacyPayRunTarget(double score)
    {
        if (score < 50.6)
        {
            return "minimal";
        }
        if (score < 57.5)
        {
            return "watch";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 53380.0, in order.</summary>
    public static int LegacyPayRunTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53380.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeLegacyPayRunTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
