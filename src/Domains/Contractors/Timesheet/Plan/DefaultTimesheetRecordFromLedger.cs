namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Contractors.Rate.Tier;

/// <summary>Functions of default timesheet record from ledger.</summary>
public static class DefaultTimesheetRecordFromLedger
{
    public static Dictionary<string, double> SummarizeRateLedgersForRecord(List<RateLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<DefaultTimesheetRecord> DeriveDefaultTimesheetRecordsFromLedger(List<RateLedger> items)
    {
        var outValue = new List<DefaultTimesheetRecord>();
        foreach (var item in items)
        {
            var derived = DefaultTimesheetRecord.NewDefaultTimesheetRecord(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectDefaultTimesheetRecord(DefaultTimesheetRecord item)
    {
        return PrimaryRateTierPolicy.PrimaryRateTierWeight(8, true);
    }
}
