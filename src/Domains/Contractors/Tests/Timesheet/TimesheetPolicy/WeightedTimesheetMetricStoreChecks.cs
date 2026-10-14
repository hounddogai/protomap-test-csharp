namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetPolicy;

using Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>Checks of timesheet policy code, run by the project's runner.</summary>
public static class WeightedTimesheetMetricStoreChecks
{
    public static void MemoryWeightedTimesheetMetricStoreFindsWhatItSaved()
    {
        var store = WeightedTimesheetMetricStore.NewMemoryWeightedTimesheetMetricStore();
        var item = WeightedTimesheetMetric.NewWeightedTimesheetMetric("code-227", "title-100");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryWeightedTimesheetMetricStoreFindsWhatItSaved();
    }
}
