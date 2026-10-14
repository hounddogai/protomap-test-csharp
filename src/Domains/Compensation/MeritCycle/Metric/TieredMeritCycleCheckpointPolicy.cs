namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered merit cycle checkpoint policy.</summary>
public static class TieredMeritCycleCheckpointPolicy
{
    public static List<string> MergeTieredMeritCycleCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int TieredMeritCycleCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a tiered merit cycle checkpoint code in its canonical TMCC- form.</summary>
    public static string NormalizeTieredMeritCycleCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TMCC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TMCC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredMeritCycleCheckpoint()
    {
        var policy = Retry.DefaultPolicy("tiered-merit-cycle-checkpoint");
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
}
