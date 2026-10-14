namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Workplace.Booking.Ledger;

/// <summary>Functions of desk record from audit.</summary>
public static class DeskRecordFromAudit
{
    public static Dictionary<string, double> SummarizeBatchedBookingAuditsForRecord(List<BatchedBookingAudit> items)
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

    public static List<DeskRecord> DeriveDeskRecordsFromAudit(List<BatchedBookingAudit> items)
    {
        var outValue = new List<DeskRecord>();
        foreach (var item in items)
        {
            var derived = DeskRecord.NewDeskRecord(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitDeskRecord(DeskRecord item)
    {
        return BookingBundlePolicy.NormalizeBookingBundleCode(item.Code);
    }
}
