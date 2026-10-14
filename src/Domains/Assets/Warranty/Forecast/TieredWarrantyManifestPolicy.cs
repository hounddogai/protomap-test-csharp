namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered warranty manifest policy.</summary>
public static class TieredWarrantyManifestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 81306.0, in order.</summary>
    public static int TieredWarrantyManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81306.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeTieredWarrantyManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a tiered warranty manifest code in its canonical TWM- form.</summary>
    public static string NormalizeTieredWarrantyManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TWM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TWM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
