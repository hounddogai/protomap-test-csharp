namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>Functions of timesheet ledger from segment.</summary>
public static class TimesheetLedgerFromSegment
{
    public static Dictionary<string, double> SummarizeVendorSegmentsForLedger(List<VendorSegment> items)
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

    public static List<TimesheetLedger> DeriveTimesheetLedgersFromSegment(List<VendorSegment> items)
    {
        var outValue = new List<TimesheetLedger>();
        foreach (var item in items)
        {
            var derived = TimesheetLedger.NewTimesheetLedger(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleTimesheetLedger(TimesheetLedger item)
    {
        return VendorSegmentPolicy.VendorSegmentBucket(item.EffectiveValue());
    }
}
