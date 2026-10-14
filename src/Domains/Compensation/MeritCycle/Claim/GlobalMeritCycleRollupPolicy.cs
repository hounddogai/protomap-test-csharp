namespace Acme.Workforce.Compensation.MeritCycle.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global merit cycle rollup policy.</summary>
public static class GlobalMeritCycleRollupPolicy
{
    public static List<string> MergeGlobalMeritCycleRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int GlobalMeritCycleRollupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Puts a global merit cycle rollup code in its canonical GMCR- form.</summary>
    public static string NormalizeGlobalMeritCycleRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GMCR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GMCR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 64985.0, in order.</summary>
    public static int GlobalMeritCycleRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64985.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
