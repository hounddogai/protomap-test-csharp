namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of nomination mapping policy.</summary>
public static class NominationMappingPolicy
{
    /// <summary>Puts a nomination mapping code in its canonical NM- form.</summary>
    public static string NormalizeNominationMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double NominationMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeNominationMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
