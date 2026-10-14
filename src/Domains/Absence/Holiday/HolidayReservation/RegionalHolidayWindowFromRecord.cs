namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Policy.Batch;

/// <summary>Functions of regional holiday window from record.</summary>
public static class RegionalHolidayWindowFromRecord
{
    public static Dictionary<string, double> SummarizePolicyRecordsForWindow(List<PolicyRecord> items)
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

    public static List<RegionalHolidayWindow> DeriveRegionalHolidayWindowsFromRecord(List<PolicyRecord> items)
    {
        var outValue = new List<RegionalHolidayWindow>();
        foreach (var item in items)
        {
            var derived = RegionalHolidayWindow.NewRegionalHolidayWindow(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileRegionalHolidayWindow(RegionalHolidayWindow item)
    {
        return PolicyRecordPolicy.NormalizePolicyRecordCode(item.Code);
    }
}
