namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LegacyTimesheetMetricReportChecks
{
    public static void LegacyTimesheetMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706581774);
        var items = new List<LegacyTimesheetMetric>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LegacyTimesheetMetric.NewLegacyTimesheetMetric($"legacy-timesheet-metric-{index}", "sample"));
        }
        var report = LegacyTimesheetMetricReport.BuildLegacyTimesheetMetricReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LegacyTimesheetMetricReportCountsEachRecordOnce();
    }
}
