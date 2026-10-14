namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary merit cycle allocation policy.</summary>
public static class PrimaryMeritCycleAllocationPolicy
{
    public static List<string> MergePrimaryMeritCycleAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string PrimaryMeritCycleAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a primary merit cycle allocation code in its canonical PMCA- form.</summary>
    public static string NormalizePrimaryMeritCycleAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PMCA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PMCA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PrimaryMeritCycleAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
