namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>Functions of primary leave request item from agreement.</summary>
public static class PrimaryLeaveRequestItemFromAgreement
{
    public static Dictionary<string, double> SummarizeLenientLeaveRequestAgreementsForItem(List<LenientLeaveRequestAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<PrimaryLeaveRequestItem> DerivePrimaryLeaveRequestItemsFromAgreement(List<LenientLeaveRequestAgreement> items)
    {
        var outValue = new List<PrimaryLeaveRequestItem>();
        foreach (var item in items)
        {
            var derived = PrimaryLeaveRequestItem.NewPrimaryLeaveRequestItem(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyPrimaryLeaveRequestItem(PrimaryLeaveRequestItem item)
    {
        return RegionalLeaveRequestItemPolicy.RegionalLeaveRequestItemBucket(item.NetValue());
    }
}
