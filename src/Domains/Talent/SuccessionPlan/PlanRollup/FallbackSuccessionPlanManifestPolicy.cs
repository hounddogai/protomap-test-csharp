namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback succession plan manifest policy.</summary>
public static class FallbackSuccessionPlanManifestPolicy
{
    public static List<string> MergeFallbackSuccessionPlanManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double FallbackSuccessionPlanManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.09;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a fallback succession plan manifest code in its canonical FSPM- form.</summary>
    public static string NormalizeFallbackSuccessionPlanManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FSPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FSPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
