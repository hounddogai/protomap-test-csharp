namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AdaptiveTimesheetDraftReportChecks
{
    public static void AdaptiveTimesheetDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704523176);
        var items = new List<AdaptiveTimesheetDraft>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptiveTimesheetDraft.NewAdaptiveTimesheetDraft($"adaptive-timesheet-draft-{index}", "sample"));
        }
        var report = AdaptiveTimesheetDraftReport.BuildAdaptiveTimesheetDraftReport("test", items, clock);
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
        AdaptiveTimesheetDraftReportCountsEachRecordOnce();
    }
}
