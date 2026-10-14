namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Functions of return plan proposal from override.</summary>
public static class ReturnPlanProposalFromOverride
{
    public static Dictionary<string, double> SummarizeLenientAccrualOverridesForProposal(List<LenientAccrualOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<ReturnPlanProposal> DeriveReturnPlanProposalsFromOverride(List<LenientAccrualOverride> items)
    {
        var outValue = new List<ReturnPlanProposal>();
        foreach (var item in items)
        {
            var derived = ReturnPlanProposal.NewReturnPlanProposal(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleReturnPlanProposal(ReturnPlanProposal item)
    {
        return RollingAccrualAssignmentPolicy.RollingAccrualAssignmentWeight(1, true);
    }
}
