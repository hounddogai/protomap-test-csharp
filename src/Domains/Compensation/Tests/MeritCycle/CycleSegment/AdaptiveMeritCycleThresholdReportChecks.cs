namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleThresholdReportChecks
{
    public static void AdaptiveMeritCycleThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701454757);
        var items = new List<AdaptiveMeritCycleThreshold>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdaptiveMeritCycleThreshold.NewAdaptiveMeritCycleThreshold($"adaptive-merit-cycle-threshold-{index}", "sample"));
        }
        var report = AdaptiveMeritCycleThresholdReport.BuildAdaptiveMeritCycleThresholdReport("test", items, clock);
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
        AdaptiveMeritCycleThresholdReportCountsEachRecordOnce();
    }
}
