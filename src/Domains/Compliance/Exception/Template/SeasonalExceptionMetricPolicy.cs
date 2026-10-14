namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal exception metric policy.</summary>
public static class SeasonalExceptionMetricPolicy
{
    public static List<string> MergeSeasonalExceptionMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int SeasonalExceptionMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Puts a seasonal exception metric code in its canonical SEM- form.</summary>
    public static string NormalizeSeasonalExceptionMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SEM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SEM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a seasonal exception metric score falls in.</summary>
    public static string ClassifySeasonalExceptionMetric(double score)
    {
        if (score < 6.0)
        {
            return "medium";
        }
        if (score < 33.4)
        {
            return "watch";
        }
        return "ok";
    }
}
