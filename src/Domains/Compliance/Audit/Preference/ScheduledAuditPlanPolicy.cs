namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled audit plan policy.</summary>
public static class ScheduledAuditPlanPolicy
{
    /// <summary>Puts a scheduled audit plan code in its canonical SAP- form.</summary>
    public static string NormalizeScheduledAuditPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SAP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SAP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ScheduledAuditPlanSpan(int start, int end)
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

    public static double ScheduledAuditPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.29;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ScheduledAuditPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
