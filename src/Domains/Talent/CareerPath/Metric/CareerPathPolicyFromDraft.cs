namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Functions of career path policy from draft.</summary>
public static class CareerPathPolicyFromDraft
{
    public static Dictionary<string, double> SummarizeRegionalSuccessionPlanDraftsForPolicy(List<RegionalSuccessionPlanDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<CareerPathPolicy> DeriveCareerPathPoliciesFromDraft(List<RegionalSuccessionPlanDraft> items)
    {
        var outValue = new List<CareerPathPolicy>();
        foreach (var item in items)
        {
            var derived = CareerPathPolicy.NewCareerPathPolicy(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchCareerPathPolicy(CareerPathPolicy item)
    {
        return RegionalSuccessionPlanDraftPolicy.ClassifyRegionalSuccessionPlanDraft(item.ProjectedLoad());
    }
}
