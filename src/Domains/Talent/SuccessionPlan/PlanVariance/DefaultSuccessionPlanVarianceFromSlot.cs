namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using Acme.Workforce.Talent.Mobility.Summary;

/// <summary>Functions of default succession plan variance from slot.</summary>
public static class DefaultSuccessionPlanVarianceFromSlot
{
    public static Dictionary<string, double> SummarizeLenientMobilitySlotsForVariance(List<LenientMobilitySlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<DefaultSuccessionPlanVariance> DeriveDefaultSuccessionPlanVariancesFromSlot(List<LenientMobilitySlot> items)
    {
        var outValue = new List<DefaultSuccessionPlanVariance>();
        foreach (var item in items)
        {
            var derived = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateDefaultSuccessionPlanVariance(DefaultSuccessionPlanVariance item)
    {
        return LenientMobilitySlotPolicy.NormalizeLenientMobilitySlotCode(item.Code);
    }
}
