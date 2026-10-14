namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of refresh cycle policy.</summary>
public static class RefreshCyclePolicy
{
    public static double RefreshCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int RefreshCycleSpan(int start, int end)
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

    public static List<string> MergeRefreshCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a refresh cycle code in its canonical RC- form.</summary>
    public static string NormalizeRefreshCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
