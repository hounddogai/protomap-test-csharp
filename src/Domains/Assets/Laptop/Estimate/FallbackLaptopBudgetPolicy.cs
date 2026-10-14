namespace Acme.Workforce.Assets.Laptop.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback laptop budget policy.</summary>
public static class FallbackLaptopBudgetPolicy
{
    /// <summary>Puts a fallback laptop budget code in its canonical FLB- form.</summary>
    public static string NormalizeFallbackLaptopBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FLB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FLB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeFallbackLaptopBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
