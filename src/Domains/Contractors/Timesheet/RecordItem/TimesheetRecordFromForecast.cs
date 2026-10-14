namespace Acme.Workforce.Contractors.Timesheet.RecordItem;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Functions of timesheet record from forecast.</summary>
public static class TimesheetRecordFromForecast
{
    public static Dictionary<string, double> SummarizeStrictOffboardingForecastsForRecord(List<StrictOffboardingForecast> items)
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

    public static List<TimesheetRecord> DeriveTimesheetRecordsFromForecast(List<StrictOffboardingForecast> items)
    {
        var outValue = new List<TimesheetRecord>();
        foreach (var item in items)
        {
            var derived = TimesheetRecord.NewTimesheetRecord(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyTimesheetRecord(TimesheetRecord item)
    {
        return LenientOffboardingProfilePolicy.NormalizeLenientOffboardingProfileCode(item.Code);
    }
}
