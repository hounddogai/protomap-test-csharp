namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default bonus pool allocation policy.</summary>
public static class DefaultBonusPoolAllocationPolicy
{
    public static string DefaultBonusPoolAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeDefaultBonusPoolAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a default bonus pool allocation code in its canonical DBPA- form.</summary>
    public static string NormalizeDefaultBonusPoolAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DBPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DBPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double DefaultBonusPoolAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
