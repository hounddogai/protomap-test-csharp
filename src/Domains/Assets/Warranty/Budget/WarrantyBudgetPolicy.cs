namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of warranty budget policy.</summary>
public static class WarrantyBudgetPolicy
{
    public static double WarrantyBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string WarrantyBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a warranty budget code in its canonical WB- form.</summary>
    public static string NormalizeWarrantyBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeWarrantyBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
