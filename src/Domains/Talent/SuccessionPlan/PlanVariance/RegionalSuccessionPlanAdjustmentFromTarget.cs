namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using Acme.Workforce.Talent.Nomination.Template;

/// <summary>Functions of regional succession plan adjustment from target.</summary>
public static class RegionalSuccessionPlanAdjustmentFromTarget
{
    public static Dictionary<string, double> SummarizeAdaptiveNominationTargetsForAdjustment(List<AdaptiveNominationTarget> items)
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

    public static List<RegionalSuccessionPlanAdjustment> DeriveRegionalSuccessionPlanAdjustmentsFromTarget(List<AdaptiveNominationTarget> items)
    {
        var outValue = new List<RegionalSuccessionPlanAdjustment>();
        foreach (var item in items)
        {
            var derived = RegionalSuccessionPlanAdjustment.NewRegionalSuccessionPlanAdjustment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeRegionalSuccessionPlanAdjustment(RegionalSuccessionPlanAdjustment item)
    {
        return AdaptiveNominationTargetPolicy.NormalizeAdaptiveNominationTargetCode(item.Code);
    }
}
