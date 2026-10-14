namespace Acme.Workforce.Compensation.BonusPool.Rule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of bonus pool rule policy.</summary>
public static class BonusPoolRulePolicy
{
    public static double BonusPoolRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a bonus pool rule code in its canonical BPR- form.</summary>
    public static string NormalizeBonusPoolRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeBonusPoolRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int BonusPoolRuleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
