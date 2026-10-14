namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted mobility record policy.</summary>
public static class WeightedMobilityRecordPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 7228.0, in order.</summary>
    public static int WeightedMobilityRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7228.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeWeightedMobilityRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double WeightedMobilityRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.2;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a weighted mobility record score falls in.</summary>
    public static string ClassifyWeightedMobilityRecord(double score)
    {
        if (score < 24.7)
        {
            return "critical";
        }
        if (score < 53.2)
        {
            return "medium";
        }
        if (score < 81.0)
        {
            return "watch";
        }
        return "minimal";
    }
}
