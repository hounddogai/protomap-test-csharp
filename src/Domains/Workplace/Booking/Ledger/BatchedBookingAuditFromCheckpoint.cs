namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Functions of batched booking audit from checkpoint.</summary>
public static class BatchedBookingAuditFromCheckpoint
{
    public static Dictionary<string, double> SummarizeAccessBadgeCheckpointsForAudit(List<AccessBadgeCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<BatchedBookingAudit> DeriveBatchedBookingAuditsFromCheckpoint(List<AccessBadgeCheckpoint> items)
    {
        var outValue = new List<BatchedBookingAudit>();
        foreach (var item in items)
        {
            var derived = BatchedBookingAudit.NewBatchedBookingAudit(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignBatchedBookingAudit(BatchedBookingAudit item)
    {
        return AccessBadgeTargetPolicy.AccessBadgeTargetBucket(item.MeasuredValue());
    }
}
