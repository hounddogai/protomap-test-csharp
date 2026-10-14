namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using Acme.Workforce.Talent.Nomination.Target;

/// <summary>Functions of succession plan quota from metric.</summary>
public static class SuccessionPlanQuotaFromMetric
{
    public static Dictionary<string, double> SummarizeNominationMetricsForQuota(List<NominationMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<SuccessionPlanQuota> DeriveSuccessionPlanQuotasFromMetric(List<NominationMetric> items)
    {
        var outValue = new List<SuccessionPlanQuota>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanQuota.NewSuccessionPlanQuota(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveSuccessionPlanQuota(SuccessionPlanQuota item)
    {
        return TieredNominationRulePolicy.ClassifyTieredNominationRule(item.AdjustedExposure());
    }
}
