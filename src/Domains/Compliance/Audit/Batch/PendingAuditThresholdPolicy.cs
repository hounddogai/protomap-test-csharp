namespace Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Functions of pending audit threshold policy.</summary>
public static class PendingAuditThresholdPolicy
{
    /// <summary>Names the band a pending audit threshold score falls in.</summary>
    public static string ClassifyPendingAuditThreshold(double score)
    {
        if (score < 42.2)
        {
            return "low";
        }
        if (score < 64.0)
        {
            return "minimal";
        }
        return "critical";
    }

    public static int PendingAuditThresholdSpan(int start, int end)
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

    /// <summary>Puts a pending audit threshold code in its canonical PAT- form.</summary>
    public static string NormalizePendingAuditThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
