namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of exception request policy.</summary>
public static class ExceptionRequestPolicy
{
    public static List<string> MergeExceptionRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double ExceptionRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a exception request score falls in.</summary>
    public static string ClassifyExceptionRequest(double score)
    {
        if (score < 7.6)
        {
            return "minimal";
        }
        if (score < 73.2)
        {
            return "low";
        }
        return "high";
    }

    /// <summary>Puts a exception request code in its canonical ER- form.</summary>
    public static string NormalizeExceptionRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ER-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ER-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
