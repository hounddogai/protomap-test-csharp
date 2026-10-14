namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy ack audit policy.</summary>
public static class PolicyAckAuditPolicy
{
    public static double PolicyAckAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PolicyAckAuditSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a policy ack audit code in its canonical PAA- form.</summary>
    public static string NormalizePolicyAckAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PolicyAckAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
