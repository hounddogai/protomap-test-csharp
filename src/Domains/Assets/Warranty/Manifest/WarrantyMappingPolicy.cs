namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of warranty mapping policy.</summary>
public static class WarrantyMappingPolicy
{
    public static string WarrantyMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 16768.0, in order.</summary>
    public static int WarrantyMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 16768.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a warranty mapping score falls in.</summary>
    public static string ClassifyWarrantyMapping(double score)
    {
        if (score < 24.6)
        {
            return "low";
        }
        if (score < 40.3)
        {
            return "critical";
        }
        if (score < 86.8)
        {
            return "elevated";
        }
        return "watch";
    }

    public static List<string> MergeWarrantyMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
