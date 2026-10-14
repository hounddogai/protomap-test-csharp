namespace Acme.Workforce.Absence.Tests.Balance.Draft;

using Acme.Workforce.Absence.Balance.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledBalanceDraftReportChecks
{
    public static void ScheduledBalanceDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701485121);
        var items = new List<ScheduledBalanceDraft>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ScheduledBalanceDraft.NewScheduledBalanceDraft($"scheduled-balance-draft-{index}", "sample"));
        }
        var report = ScheduledBalanceDraftReport.BuildScheduledBalanceDraftReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScheduledBalanceDraftReportCountsEachRecordOnce();
    }
}
