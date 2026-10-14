namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Request;

/// <summary>Functions of succession plan draft from request.</summary>
public static class SuccessionPlanDraftFromRequest
{
    public static Dictionary<string, double> SummarizeGlobalSuccessionPlanRequestsForDraft(List<GlobalSuccessionPlanRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<SuccessionPlanDraft> DeriveSuccessionPlanDraftsFromRequest(List<GlobalSuccessionPlanRequest> items)
    {
        var outValue = new List<SuccessionPlanDraft>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanDraft.NewSuccessionPlanDraft(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignSuccessionPlanDraft(SuccessionPlanDraft item)
    {
        return GlobalSuccessionPlanRequestPolicy.NormalizeGlobalSuccessionPlanRequestCode(item.Code);
    }
}
