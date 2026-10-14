namespace Acme.Workforce.Workplace.Tests.Desk.DeskRecord;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>Checks of desk record code, run by the project's runner.</summary>
public static class PendingDeskBaselineReportChecks
{
    public static void PendingDeskBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705218454);
        var items = new List<PendingDeskBaseline>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PendingDeskBaseline.NewPendingDeskBaseline($"pending-desk-baseline-{index}", "sample"));
        }
        var report = PendingDeskBaselineReport.BuildPendingDeskBaselineReport("test", items, clock);
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
        PendingDeskBaselineReportCountsEachRecordOnce();
    }
}
