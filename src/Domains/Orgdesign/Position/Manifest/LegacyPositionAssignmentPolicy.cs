namespace Acme.Workforce.Orgdesign.Position.Manifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy position assignment policy.</summary>
public static class LegacyPositionAssignmentPolicy
{
    /// <summary>Puts a legacy position assignment code in its canonical LPA- form.</summary>
    public static string NormalizeLegacyPositionAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LegacyPositionAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeLegacyPositionAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a legacy position assignment score falls in.</summary>
    public static string ClassifyLegacyPositionAssignment(double score)
    {
        if (score < 12.7)
        {
            return "ok";
        }
        if (score < 83.1)
        {
            return "minimal";
        }
        return "low";
    }
}
