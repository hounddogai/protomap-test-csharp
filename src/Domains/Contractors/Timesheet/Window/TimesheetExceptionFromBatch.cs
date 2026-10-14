namespace Acme.Workforce.Contractors.Timesheet.Window;

using Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Functions of timesheet exception from batch.</summary>
public static class TimesheetExceptionFromBatch
{
    public static Dictionary<string, double> SummarizeLegacyVendorBatchesForException(List<LegacyVendorBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<TimesheetException> DeriveTimesheetExceptionsFromBatch(List<LegacyVendorBatch> items)
    {
        var outValue = new List<TimesheetException>();
        foreach (var item in items)
        {
            var derived = TimesheetException.NewTimesheetException(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateTimesheetException(TimesheetException item)
    {
        return LegacyVendorBatchPolicy.ClassifyLegacyVendorBatch(item.AdjustedLoad());
    }
}
