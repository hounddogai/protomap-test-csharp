namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling asset return assignment policy.</summary>
public static class RollingAssetReturnAssignmentPolicy
{
    public static List<string> MergeRollingAssetReturnAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double RollingAssetReturnAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a rolling asset return assignment code in its canonical RARA- form.</summary>
    public static string NormalizeRollingAssetReturnAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RARA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RARA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RollingAssetReturnAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
