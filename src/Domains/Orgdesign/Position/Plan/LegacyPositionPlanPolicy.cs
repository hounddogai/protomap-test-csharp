namespace Acme.Workforce.Orgdesign.Position.Plan;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy position plan policy.</summary>
public static class LegacyPositionPlanPolicy
{
    public static string LegacyPositionPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static List<string> MergeLegacyPositionPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a legacy position plan code in its canonical LPP- form.</summary>
    public static string NormalizeLegacyPositionPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
