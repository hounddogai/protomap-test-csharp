namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default audit tier policy.</summary>
public static class DefaultAuditTierPolicy
{
    public static int DefaultAuditTierSpan(int start, int end)
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
        return span / 86400;
    }

    public static string DefaultAuditTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
