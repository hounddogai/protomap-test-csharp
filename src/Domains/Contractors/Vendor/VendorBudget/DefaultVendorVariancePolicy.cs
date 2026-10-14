namespace Acme.Workforce.Contractors.Vendor.VendorBudget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default vendor variance policy.</summary>
public static class DefaultVendorVariancePolicy
{
    public static string DefaultVendorVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a default vendor variance score falls in.</summary>
    public static string ClassifyDefaultVendorVariance(double score)
    {
        if (score < 55.3)
        {
            return "medium";
        }
        if (score < 65.7)
        {
            return "minimal";
        }
        return "elevated";
    }

    /// <summary>Puts a default vendor variance code in its canonical DVV- form.</summary>
    public static string NormalizeDefaultVendorVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DVV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DVV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeDefaultVendorVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
