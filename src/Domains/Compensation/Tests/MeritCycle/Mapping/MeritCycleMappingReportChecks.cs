namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class MeritCycleMappingReportChecks
{
    public static void MeritCycleMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703411060);
        var items = new List<MeritCycleMapping>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(MeritCycleMapping.NewMeritCycleMapping($"merit-cycle-mapping-{index}", "sample"));
        }
        var report = MeritCycleMappingReport.BuildMeritCycleMappingReport("test", items, clock);
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
        MeritCycleMappingReportCountsEachRecordOnce();
    }
}
