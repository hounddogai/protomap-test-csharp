namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of bonus pool entry policy.</summary>
public static class BonusPoolEntryPolicy
{
    /// <summary>Puts a bonus pool entry code in its canonical BPE- form.</summary>
    public static string NormalizeBonusPoolEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeBonusPoolEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double BonusPoolEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
