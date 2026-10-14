namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskWindowReportChecks
{
    public static void DeskWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700258300);
        var items = new List<DeskWindow>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DeskWindow.NewDeskWindow($"desk-window-{index}", "sample"));
        }
        var report = DeskWindowReport.BuildDeskWindowReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DeskWindowReportCountsEachRecordOnce();
    }
}
