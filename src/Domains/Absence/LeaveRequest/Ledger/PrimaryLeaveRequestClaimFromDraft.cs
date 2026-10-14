namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Audit;

/// <summary>Functions of primary leave request claim from draft.</summary>
public static class PrimaryLeaveRequestClaimFromDraft
{
    public static Dictionary<string, double> SummarizeReturnPlanDraftsForClaim(List<ReturnPlanDraft> items)
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

    public static List<PrimaryLeaveRequestClaim> DerivePrimaryLeaveRequestClaimsFromDraft(List<ReturnPlanDraft> items)
    {
        var outValue = new List<PrimaryLeaveRequestClaim>();
        foreach (var item in items)
        {
            var derived = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyPrimaryLeaveRequestClaim(PrimaryLeaveRequestClaim item)
    {
        return ReturnPlanDraftPolicy.ReturnPlanDraftBucket(item.EffectiveTotal());
    }
}
