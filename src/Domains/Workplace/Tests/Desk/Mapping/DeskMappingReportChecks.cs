namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskMappingReportChecks
{
    public static void DeskMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708301061);
        var items = new List<DeskMapping>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DeskMapping.NewDeskMapping($"desk-mapping-{index}", "sample"));
        }
        var report = DeskMappingReport.BuildDeskMappingReport("test", items, clock);
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
        DeskMappingReportCountsEachRecordOnce();
    }
}
