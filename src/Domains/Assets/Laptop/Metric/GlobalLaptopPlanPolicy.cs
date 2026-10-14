namespace Acme.Workforce.Assets.Laptop.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global laptop plan policy.</summary>
public static class GlobalLaptopPlanPolicy
{
    public static List<string> MergeGlobalLaptopPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a global laptop plan code in its canonical GLP- form.</summary>
    public static string NormalizeGlobalLaptopPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GLP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GLP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
