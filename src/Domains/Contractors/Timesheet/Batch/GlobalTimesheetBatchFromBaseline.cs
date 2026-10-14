namespace Acme.Workforce.Contractors.Timesheet.Batch;

using Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>Functions of global timesheet batch from baseline.</summary>
public static class GlobalTimesheetBatchFromBaseline
{
    public static Dictionary<string, double> SummarizeOffboardingBaselinesForBatch(List<OffboardingBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<GlobalTimesheetBatch> DeriveGlobalTimesheetBatchesFromBaseline(List<OffboardingBaseline> items)
    {
        var outValue = new List<GlobalTimesheetBatch>();
        foreach (var item in items)
        {
            var derived = GlobalTimesheetBatch.NewGlobalTimesheetBatch(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeGlobalTimesheetBatch(GlobalTimesheetBatch item)
    {
        return LenientOffboardingForecastPolicy.NormalizeLenientOffboardingForecastCode(item.Code);
    }
}
