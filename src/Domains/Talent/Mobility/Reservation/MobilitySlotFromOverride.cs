namespace Acme.Workforce.Talent.Mobility.Reservation;

using Acme.Workforce.Talent.CareerPath.Plan;

/// <summary>Functions of mobility slot from override.</summary>
public static class MobilitySlotFromOverride
{
    public static Dictionary<string, double> SummarizeWeightedCareerPathOverridesForSlot(List<WeightedCareerPathOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<MobilitySlot> DeriveMobilitySlotsFromOverride(List<WeightedCareerPathOverride> items)
    {
        var outValue = new List<MobilitySlot>();
        foreach (var item in items)
        {
            var derived = MobilitySlot.NewMobilitySlot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectMobilitySlot(MobilitySlot item)
    {
        return WeightedCareerPathOverridePolicy.WeightedCareerPathOverrideWeight(2, true);
    }
}
