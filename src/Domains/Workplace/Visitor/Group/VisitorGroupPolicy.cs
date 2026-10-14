namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of visitor group policy.</summary>
public static class VisitorGroupPolicy
{
    public static string VisitorGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeVisitorGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a visitor group code in its canonical VG- form.</summary>
    public static string NormalizeVisitorGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a visitor group score falls in.</summary>
    public static string ClassifyVisitorGroup(double score)
    {
        if (score < 17.9)
        {
            return "minimal";
        }
        if (score < 28.4)
        {
            return "watch";
        }
        if (score < 30.8)
        {
            return "high";
        }
        if (score < 79.4)
        {
            return "elevated";
        }
        return "critical";
    }
}
