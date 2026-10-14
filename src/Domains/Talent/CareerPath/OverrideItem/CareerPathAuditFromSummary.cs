namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Functions of career path audit from summary.</summary>
public static class CareerPathAuditFromSummary
{
    public static Dictionary<string, double> SummarizeRegionalSuccessionPlanSummariesForAudit(List<RegionalSuccessionPlanSummary> items)
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

    public static List<CareerPathAudit> DeriveCareerPathAuditsFromSummary(List<RegionalSuccessionPlanSummary> items)
    {
        var outValue = new List<CareerPathAudit>();
        foreach (var item in items)
        {
            var derived = CareerPathAudit.NewCareerPathAudit(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveCareerPathAudit(CareerPathAudit item)
    {
        return SuccessionPlanProposalPolicy.SuccessionPlanProposalBucket(item.AdjustedVolume());
    }
}
