namespace Acme.Workforce.Talent.CareerPath.Plan;

using Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>Functions of weighted career path override from threshold.</summary>
public static class WeightedCareerPathOverrideFromThreshold
{
    public static Dictionary<string, double> SummarizeRollingNominationThresholdsForOverride(List<RollingNominationThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<WeightedCareerPathOverride> DeriveWeightedCareerPathOverridesFromThreshold(List<RollingNominationThreshold> items)
    {
        var outValue = new List<WeightedCareerPathOverride>();
        foreach (var item in items)
        {
            var derived = WeightedCareerPathOverride.NewWeightedCareerPathOverride(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateWeightedCareerPathOverride(WeightedCareerPathOverride item)
    {
        return RollingNominationThresholdPolicy.RollingNominationThresholdWeight(5, true);
    }
}
