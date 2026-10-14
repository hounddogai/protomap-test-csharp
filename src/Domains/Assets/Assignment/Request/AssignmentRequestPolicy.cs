namespace Acme.Workforce.Assets.Assignment.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of assignment request policy.</summary>
public static class AssignmentRequestPolicy
{
    /// <summary>Names the band a assignment request score falls in.</summary>
    public static string ClassifyAssignmentRequest(double score)
    {
        if (score < 6.8)
        {
            return "low";
        }
        if (score < 21.6)
        {
            return "high";
        }
        if (score < 37.9)
        {
            return "critical";
        }
        if (score < 94.3)
        {
            return "elevated";
        }
        return "ok";
    }

    public static string AssignmentRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeAssignmentRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a assignment request code in its canonical AR- form.</summary>
    public static string NormalizeAssignmentRequestCode(string code)
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
