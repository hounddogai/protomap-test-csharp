namespace Acme.Workforce.Compensation.MeritCycle.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of merit cycle audit policy.</summary>
public static class MeritCycleAuditPolicy
{
    public static List<string> MergeMeritCycleAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a merit cycle audit code in its canonical MCA- form.</summary>
    public static string NormalizeMeritCycleAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MCA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MCA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double MeritCycleAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.06;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int MeritCycleAuditSpan(int start, int end)
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
        return span / 60;
    }
}
