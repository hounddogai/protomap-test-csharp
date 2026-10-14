namespace Acme.Workforce.Absence.Balance.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global balance policy policy.</summary>
public static class GlobalBalancePolicyPolicy
{
    /// <summary>Names the band a global balance policy score falls in.</summary>
    public static string ClassifyGlobalBalancePolicy(double score)
    {
        if (score < 2.8)
        {
            return "ok";
        }
        if (score < 68.8)
        {
            return "critical";
        }
        return "minimal";
    }

    public static List<string> MergeGlobalBalancePolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a global balance policy code in its canonical GBP- form.</summary>
    public static string NormalizeGlobalBalancePolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GBP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GBP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double GlobalBalancePolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.5;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
