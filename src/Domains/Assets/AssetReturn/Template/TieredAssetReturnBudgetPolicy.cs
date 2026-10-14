namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered asset return budget policy.</summary>
public static class TieredAssetReturnBudgetPolicy
{
    /// <summary>Puts a tiered asset return budget code in its canonical TARB- form.</summary>
    public static string NormalizeTieredAssetReturnBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TARB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TARB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double TieredAssetReturnBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.77;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeTieredAssetReturnBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
