namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of warranty segment policy.</summary>
public static class WarrantySegmentPolicy
{
    /// <summary>Puts a warranty segment code in its canonical WS- form.</summary>
    public static string NormalizeWarrantySegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeWarrantySegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 67438.0, in order.</summary>
    public static int WarrantySegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67438.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double WarrantySegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.29;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
