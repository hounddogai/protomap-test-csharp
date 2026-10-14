namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Functions of batched garnishment metric policy.</summary>
public static class BatchedGarnishmentMetricPolicy
{
    /// <summary>Puts a batched garnishment metric code in its canonical BGM- form.</summary>
    public static string NormalizeBatchedGarnishmentMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BGM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BGM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int BatchedGarnishmentMetricSpan(int start, int end)
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
        return span / 86400;
    }
}
