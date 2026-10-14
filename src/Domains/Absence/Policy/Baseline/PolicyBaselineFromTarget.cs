namespace Acme.Workforce.Absence.Policy.Baseline;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Functions of policy baseline from target.</summary>
public static class PolicyBaselineFromTarget
{
    public static Dictionary<string, double> SummarizeHolidayTargetsForBaseline(List<HolidayTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<PolicyBaseline> DerivePolicyBaselinesFromTarget(List<HolidayTarget> items)
    {
        var outValue = new List<PolicyBaseline>();
        foreach (var item in items)
        {
            var derived = PolicyBaseline.NewPolicyBaseline(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScorePolicyBaseline(PolicyBaseline item)
    {
        return WeightedHolidayMappingPolicy.WeightedHolidayMappingWeight(9, true);
    }
}
