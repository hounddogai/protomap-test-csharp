namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling audit plan policy.</summary>
public static class RollingAuditPlanPolicy
{
    public static List<string> MergeRollingAuditPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int RollingAuditPlanSpan(int start, int end)
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

    public static double RollingAuditPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a rolling audit plan code in its canonical RAP- form.</summary>
    public static string NormalizeRollingAuditPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RAP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RAP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
