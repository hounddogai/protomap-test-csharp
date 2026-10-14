namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>Functions of career path preference from adjustment.</summary>
public static class CareerPathPreferenceFromAdjustment
{
    public static Dictionary<string, double> SummarizeSuccessionPlanAdjustmentsForPreference(List<SuccessionPlanAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<CareerPathPreference> DeriveCareerPathPreferencesFromAdjustment(List<SuccessionPlanAdjustment> items)
    {
        var outValue = new List<CareerPathPreference>();
        foreach (var item in items)
        {
            var derived = CareerPathPreference.NewCareerPathPreference(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateCareerPathPreference(CareerPathPreference item)
    {
        return RollingSuccessionPlanLedgerPolicy.RollingSuccessionPlanLedgerWeight(8, true);
    }
}
