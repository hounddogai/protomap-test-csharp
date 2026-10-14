namespace Acme.Workforce.Compliance.Exception.Baseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of exception baseline policy.</summary>
public static class ExceptionBaselinePolicy
{
    /// <summary>Names the band a exception baseline score falls in.</summary>
    public static string ClassifyExceptionBaseline(double score)
    {
        if (score < 20.8)
        {
            return "critical";
        }
        if (score < 34.1)
        {
            return "watch";
        }
        return "ok";
    }

    /// <summary>Puts a exception baseline code in its canonical EB- form.</summary>
    public static string NormalizeExceptionBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeExceptionBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
