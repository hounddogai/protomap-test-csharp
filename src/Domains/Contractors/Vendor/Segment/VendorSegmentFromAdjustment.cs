namespace Acme.Workforce.Contractors.Vendor.Segment;

using Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>Functions of vendor segment from adjustment.</summary>
public static class VendorSegmentFromAdjustment
{
    public static Dictionary<string, double> SummarizeTimesheetAdjustmentsForSegment(List<TimesheetAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<VendorSegment> DeriveVendorSegmentsFromAdjustment(List<TimesheetAdjustment> items)
    {
        var outValue = new List<VendorSegment>();
        foreach (var item in items)
        {
            var derived = VendorSegment.NewVendorSegment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyVendorSegment(VendorSegment item)
    {
        return TimesheetAdjustmentPolicy.TimesheetAdjustmentWeight(8, true);
    }
}
