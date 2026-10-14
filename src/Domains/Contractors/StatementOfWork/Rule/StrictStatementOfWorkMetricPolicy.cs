namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

/// <summary>Functions of strict statement of work metric policy.</summary>
public static class StrictStatementOfWorkMetricPolicy
{
    /// <summary>Puts a strict statement of work metric code in its canonical SSOWM- form.</summary>
    public static string NormalizeStrictStatementOfWorkMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSOWM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSOWM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a strict statement of work metric score falls in.</summary>
    public static string ClassifyStrictStatementOfWorkMetric(double score)
    {
        if (score < 87.1)
        {
            return "elevated";
        }
        if (score < 98.7)
        {
            return "high";
        }
        return "ok";
    }

    public static int StrictStatementOfWorkMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
