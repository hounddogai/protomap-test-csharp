namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of merit cycle bundle policy.</summary>
public static class MeritCycleBundlePolicy
{
    public static string MeritCycleBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a merit cycle bundle code in its canonical MCB- form.</summary>
    public static string NormalizeMeritCycleBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MCB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MCB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeMeritCycleBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int MeritCycleBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }
}
