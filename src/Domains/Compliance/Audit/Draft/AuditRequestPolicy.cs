namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of audit request policy.</summary>
public static class AuditRequestPolicy
{
    /// <summary>Names the band a audit request score falls in.</summary>
    public static string ClassifyAuditRequest(double score)
    {
        if (score < 21.8)
        {
            return "elevated";
        }
        if (score < 41.9)
        {
            return "medium";
        }
        if (score < 70.0)
        {
            return "minimal";
        }
        if (score < 72.5)
        {
            return "critical";
        }
        return "high";
    }

    public static int AuditRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static string AuditRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a audit request code in its canonical AR- form.</summary>
    public static string NormalizeAuditRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
