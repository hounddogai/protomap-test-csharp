namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted garnishment milestone policy.</summary>
public static class WeightedGarnishmentMilestonePolicy
{
    public static List<string> MergeWeightedGarnishmentMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string WeightedGarnishmentMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
