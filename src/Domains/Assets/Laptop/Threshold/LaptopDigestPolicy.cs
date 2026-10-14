namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of laptop digest policy.</summary>
public static class LaptopDigestPolicy
{
    public static List<string> MergeLaptopDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 51453.0, in order.</summary>
    public static int LaptopDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 51453.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a laptop digest score falls in.</summary>
    public static string ClassifyLaptopDigest(double score)
    {
        if (score < 4.2)
        {
            return "medium";
        }
        if (score < 5.8)
        {
            return "high";
        }
        return "critical";
    }
}
