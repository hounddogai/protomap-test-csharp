namespace Acme.Workforce.Workplace.Tests.Desk.Profile;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DeskRollupReportChecks
{
    public static void DeskRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705761707);
        var items = new List<DeskRollup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DeskRollup.NewDeskRollup($"desk-rollup-{index}", "sample"));
        }
        var report = DeskRollupReport.BuildDeskRollupReport("test", items, clock);
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
        DeskRollupReportCountsEachRecordOnce();
    }
}
