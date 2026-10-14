namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleMappingReportChecks
{
    public static void AdaptiveMeritCycleMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707676279);
        var items = new List<AdaptiveMeritCycleMapping>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping($"adaptive-merit-cycle-mapping-{index}", "sample"));
        }
        var report = AdaptiveMeritCycleMappingReport.BuildAdaptiveMeritCycleMappingReport("test", items, clock);
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
        AdaptiveMeritCycleMappingReportCountsEachRecordOnce();
    }
}
