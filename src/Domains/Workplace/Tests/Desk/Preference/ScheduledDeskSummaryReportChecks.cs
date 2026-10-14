namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledDeskSummaryReportChecks
{
    public static void ScheduledDeskSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707527842);
        var items = new List<ScheduledDeskSummary>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledDeskSummary.NewScheduledDeskSummary($"scheduled-desk-summary-{index}", "sample"));
        }
        var report = ScheduledDeskSummaryReport.BuildScheduledDeskSummaryReport("test", items, clock);
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
        ScheduledDeskSummaryReportCountsEachRecordOnce();
    }
}
