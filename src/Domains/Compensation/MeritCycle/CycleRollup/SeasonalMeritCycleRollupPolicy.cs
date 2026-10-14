namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal merit cycle rollup policy.</summary>
public static class SeasonalMeritCycleRollupPolicy
{
    /// <summary>Puts a seasonal merit cycle rollup code in its canonical SMCR- form.</summary>
    public static string NormalizeSeasonalMeritCycleRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SMCR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SMCR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalMeritCycleRollup()
    {
        var policy = Retry.DefaultPolicy("seasonal-merit-cycle-rollup");
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

    public static List<string> MergeSeasonalMeritCycleRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 14988.0, in order.</summary>
    public static int SeasonalMeritCycleRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14988.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
