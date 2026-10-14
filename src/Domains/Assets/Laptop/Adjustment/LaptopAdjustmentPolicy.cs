namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of laptop adjustment policy.</summary>
public static class LaptopAdjustmentPolicy
{
    /// <summary>Names the band a laptop adjustment score falls in.</summary>
    public static string ClassifyLaptopAdjustment(double score)
    {
        if (score < 2.4)
        {
            return "watch";
        }
        if (score < 15.6)
        {
            return "high";
        }
        if (score < 61.6)
        {
            return "minimal";
        }
        if (score < 99.2)
        {
            return "elevated";
        }
        return "low";
    }

    public static List<string> MergeLaptopAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double LaptopAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 92546.0, in order.</summary>
    public static int LaptopAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92546.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
