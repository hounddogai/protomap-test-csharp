namespace Acme.Workforce.Absence.Holiday.Proposal;

using Acme.Workforce.Absence.Balance.Segment;

/// <summary>Functions of holiday adjustment from policy.</summary>
public static class HolidayAdjustmentFromPolicy
{
    public static Dictionary<string, double> SummarizeBalancePoliciesForAdjustment(List<BalancePolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<HolidayAdjustment> DeriveHolidayAdjustmentsFromPolicy(List<BalancePolicy> items)
    {
        var outValue = new List<HolidayAdjustment>();
        foreach (var item in items)
        {
            var derived = HolidayAdjustment.NewHolidayAdjustment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleHolidayAdjustment(HolidayAdjustment item)
    {
        return BalancePolicyPolicy.ClassifyBalancePolicy(item.ProjectedVolume());
    }
}
