namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Assets.Assignment.AssignmentAllocation;

/// <summary>Functions of capped refresh slot from reservation.</summary>
public static class CappedRefreshSlotFromReservation
{
    public static Dictionary<string, double> SummarizeAssignmentReservationsForSlot(List<AssignmentReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<CappedRefreshSlot> DeriveCappedRefreshSlotsFromReservation(List<AssignmentReservation> items)
    {
        var outValue = new List<CappedRefreshSlot>();
        foreach (var item in items)
        {
            var derived = CappedRefreshSlot.NewCappedRefreshSlot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyCappedRefreshSlot(CappedRefreshSlot item)
    {
        return AssignmentItemPolicy.NormalizeAssignmentItemCode(item.Code);
    }
}
