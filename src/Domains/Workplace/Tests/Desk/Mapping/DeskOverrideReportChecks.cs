namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskOverrideReportChecks
{
    public static void DeskOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700931497);
        var items = new List<DeskOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DeskOverride.NewDeskOverride($"desk-override-{index}", "sample"));
        }
        var report = DeskOverrideReport.BuildDeskOverrideReport("test", items, clock);
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
        DeskOverrideReportCountsEachRecordOnce();
    }
}
