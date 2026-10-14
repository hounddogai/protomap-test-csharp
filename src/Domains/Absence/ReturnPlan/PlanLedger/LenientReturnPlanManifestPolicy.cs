namespace Acme.Workforce.Absence.ReturnPlan.PlanLedger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient return plan manifest policy.</summary>
public static class LenientReturnPlanManifestPolicy
{
    /// <summary>Names the band a lenient return plan manifest score falls in.</summary>
    public static string ClassifyLenientReturnPlanManifest(double score)
    {
        if (score < 1.4)
        {
            return "low";
        }
        if (score < 93.2)
        {
            return "high";
        }
        if (score < 96.8)
        {
            return "ok";
        }
        return "elevated";
    }

    public static List<string> MergeLenientReturnPlanManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a lenient return plan manifest code in its canonical LRPM- form.</summary>
    public static string NormalizeLenientReturnPlanManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
