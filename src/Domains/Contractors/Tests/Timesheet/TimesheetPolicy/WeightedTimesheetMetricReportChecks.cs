namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetPolicy;

using Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of timesheet policy code, run by the project's runner.</summary>
public static class WeightedTimesheetMetricReportChecks
{
    public static void WeightedTimesheetMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705271115);
        var items = new List<WeightedTimesheetMetric>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedTimesheetMetric.NewWeightedTimesheetMetric($"weighted-timesheet-metric-{index}", "sample"));
        }
        var report = WeightedTimesheetMetricReport.BuildWeightedTimesheetMetricReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WeightedTimesheetMetricReportCountsEachRecordOnce();
    }
}
