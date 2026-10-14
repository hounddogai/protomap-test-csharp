namespace Acme.Workforce.Absence.ReturnPlan.Audit;

using Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

/// <summary>Functions of return plan draft from manifest.</summary>
public static class ReturnPlanDraftFromManifest
{
    public static Dictionary<string, double> SummarizeStrictReturnPlanManifestsForDraft(List<StrictReturnPlanManifest> items)
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

    public static List<ReturnPlanDraft> DeriveReturnPlanDraftsFromManifest(List<StrictReturnPlanManifest> items)
    {
        var outValue = new List<ReturnPlanDraft>();
        foreach (var item in items)
        {
            var derived = ReturnPlanDraft.NewReturnPlanDraft(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeReturnPlanDraft(ReturnPlanDraft item)
    {
        return ReturnPlanNoticePolicy.NormalizeReturnPlanNoticeCode(item.Code);
    }
}
