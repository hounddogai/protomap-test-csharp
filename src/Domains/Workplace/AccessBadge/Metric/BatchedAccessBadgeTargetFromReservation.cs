namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Functions of batched access badge target from reservation.</summary>
public static class BatchedAccessBadgeTargetFromReservation
{
    public static Dictionary<string, double> SummarizeDeskReservationsForTarget(List<DeskReservation> items)
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

    public static List<BatchedAccessBadgeTarget> DeriveBatchedAccessBadgeTargetsFromReservation(List<DeskReservation> items)
    {
        var outValue = new List<BatchedAccessBadgeTarget>();
        foreach (var item in items)
        {
            var derived = BatchedAccessBadgeTarget.NewBatchedAccessBadgeTarget(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveBatchedAccessBadgeTarget(BatchedAccessBadgeTarget item)
    {
        return DeskSlotPolicy.DeskSlotWeight(7, true);
    }
}
