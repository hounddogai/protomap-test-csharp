namespace Acme.Workforce.Workplace.FloorPlan.Agreement;

using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Functions of floor plan agreement from policy.</summary>
public static class FloorPlanAgreementFromPolicy
{
    public static Dictionary<string, double> SummarizeVisitorPoliciesForAgreement(List<VisitorPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<FloorPlanAgreement> DeriveFloorPlanAgreementsFromPolicy(List<VisitorPolicy> items)
    {
        var outValue = new List<FloorPlanAgreement>();
        foreach (var item in items)
        {
            var derived = FloorPlanAgreement.NewFloorPlanAgreement(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanFloorPlanAgreement(FloorPlanAgreement item)
    {
        return VisitorAgreementPolicy.ClassifyVisitorAgreement(item.AdjustedValue());
    }
}
