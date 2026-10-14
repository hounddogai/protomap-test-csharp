namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

/// <summary>Functions of mobility preference from variance.</summary>
public static class MobilityPreferenceFromVariance
{
    public static Dictionary<string, double> SummarizeDefaultSuccessionPlanVariancesForPreference(List<DefaultSuccessionPlanVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<MobilityPreference> DeriveMobilityPreferencesFromVariance(List<DefaultSuccessionPlanVariance> items)
    {
        var outValue = new List<MobilityPreference>();
        foreach (var item in items)
        {
            var derived = MobilityPreference.NewMobilityPreference(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignMobilityPreference(MobilityPreference item)
    {
        return DefaultSuccessionPlanVariancePolicy.DefaultSuccessionPlanVarianceBucket(item.ProjectedValue());
    }
}
