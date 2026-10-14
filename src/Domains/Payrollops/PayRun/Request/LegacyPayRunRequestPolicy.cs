namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy pay run request policy.</summary>
public static class LegacyPayRunRequestPolicy
{
    public static List<string> MergeLegacyPayRunRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int LegacyPayRunRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    public static double LegacyPayRunRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.48;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a legacy pay run request code in its canonical LPRR- form.</summary>
    public static string NormalizeLegacyPayRunRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
