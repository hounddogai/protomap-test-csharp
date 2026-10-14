namespace Acme.Workforce.Compliance.Exception.Group;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of exception plan policy.</summary>
public static class ExceptionPlanPolicy
{
    public static int ExceptionPlanSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a exception plan score falls in.</summary>
    public static string ClassifyExceptionPlan(double score)
    {
        if (score < 1.8)
        {
            return "low";
        }
        if (score < 85.7)
        {
            return "elevated";
        }
        if (score < 90.5)
        {
            return "high";
        }
        if (score < 93.1)
        {
            return "ok";
        }
        return "minimal";
    }

    /// <summary>Puts a exception plan code in its canonical EP- form.</summary>
    public static string NormalizeExceptionPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeExceptionPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
