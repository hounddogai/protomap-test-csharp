namespace Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched return plan manifest policy.</summary>
public static class BatchedReturnPlanManifestPolicy
{
    public static string BatchedReturnPlanManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a batched return plan manifest code in its canonical BRPM- form.</summary>
    public static string NormalizeBatchedReturnPlanManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BRPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BRPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeBatchedReturnPlanManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
