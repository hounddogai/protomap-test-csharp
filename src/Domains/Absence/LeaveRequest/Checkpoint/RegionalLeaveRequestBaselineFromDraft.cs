namespace Acme.Workforce.Absence.LeaveRequest.Checkpoint;

using Acme.Workforce.Absence.Balance.Assignment;

/// <summary>Functions of regional leave request baseline from draft.</summary>
public static class RegionalLeaveRequestBaselineFromDraft
{
    public static Dictionary<string, double> SummarizeBalanceDraftsForBaseline(List<BalanceDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<RegionalLeaveRequestBaseline> DeriveRegionalLeaveRequestBaselinesFromDraft(List<BalanceDraft> items)
    {
        var outValue = new List<RegionalLeaveRequestBaseline>();
        foreach (var item in items)
        {
            var derived = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyRegionalLeaveRequestBaseline(RegionalLeaveRequestBaseline item)
    {
        return BalanceProposalPolicy.NormalizeBalanceProposalCode(item.Code);
    }
}
