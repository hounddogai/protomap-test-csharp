namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>Functions of regional pay run batch policy.</summary>
public static class RegionalPayRunBatchPolicy
{
    /// <summary>Names the band a regional pay run batch score falls in.</summary>
    public static string ClassifyRegionalPayRunBatch(double score)
    {
        if (score < 24.2)
        {
            return "critical";
        }
        if (score < 84.8)
        {
            return "medium";
        }
        return "high";
    }

    /// <summary>Puts a regional pay run batch code in its canonical RPRB- form.</summary>
    public static string NormalizeRegionalPayRunBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPRB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPRB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RegionalPayRunBatchSpan(int start, int end)
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
        return span / 3600;
    }
}
