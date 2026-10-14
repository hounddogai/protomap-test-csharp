namespace Acme.Workforce.Compliance.Exception.Assignment;

/// <summary>Functions of exception metric policy.</summary>
public static class ExceptionMetricPolicy
{
    /// <summary>Names the band a exception metric score falls in.</summary>
    public static string ClassifyExceptionMetric(double score)
    {
        if (score < 82.3)
        {
            return "high";
        }
        if (score < 92.1)
        {
            return "ok";
        }
        if (score < 97.4)
        {
            return "elevated";
        }
        if (score < 97.8)
        {
            return "medium";
        }
        return "minimal";
    }

    /// <summary>Puts a exception metric code in its canonical EM- form.</summary>
    public static string NormalizeExceptionMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
