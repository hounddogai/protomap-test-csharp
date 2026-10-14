namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of bonus pool claim policy.</summary>
public static class BonusPoolClaimPolicy
{
    public static List<string> MergeBonusPoolClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double BonusPoolClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int BonusPoolClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Puts a bonus pool claim code in its canonical BPC- form.</summary>
    public static string NormalizeBonusPoolClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
