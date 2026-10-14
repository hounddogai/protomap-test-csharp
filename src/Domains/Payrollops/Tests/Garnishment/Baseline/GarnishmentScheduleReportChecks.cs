namespace Acme.Workforce.Payrollops.Tests.Garnishment.Baseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GarnishmentScheduleReportChecks
{
    public static void GarnishmentScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705562606);
        var items = new List<GarnishmentSchedule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(GarnishmentSchedule.NewGarnishmentSchedule($"garnishment-schedule-{index}", "sample"));
        }
        var report = GarnishmentScheduleReport.BuildGarnishmentScheduleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        GarnishmentScheduleReportCountsEachRecordOnce();
    }
}
