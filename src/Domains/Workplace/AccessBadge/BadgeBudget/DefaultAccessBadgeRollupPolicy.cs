namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default access badge rollup policy.</summary>
public static class DefaultAccessBadgeRollupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 87873.0, in order.</summary>
    public static int DefaultAccessBadgeRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87873.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a default access badge rollup score falls in.</summary>
    public static string ClassifyDefaultAccessBadgeRollup(double score)
    {
        if (score < 23.2)
        {
            return "watch";
        }
        if (score < 72.5)
        {
            return "minimal";
        }
        if (score < 77.3)
        {
            return "critical";
        }
        if (score < 87.7)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultAccessBadgeRollup()
    {
        var policy = Retry.DefaultPolicy("default-access-badge-rollup");
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

    public static List<string> MergeDefaultAccessBadgeRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
