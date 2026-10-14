namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RollingMeritCycleSegmentReportChecks
{
    public static void RollingMeritCycleSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703814150);
        var items = new List<RollingMeritCycleSegment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RollingMeritCycleSegment.NewRollingMeritCycleSegment($"rolling-merit-cycle-segment-{index}", "sample"));
        }
        var report = RollingMeritCycleSegmentReport.BuildRollingMeritCycleSegmentReport("test", items, clock);
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
        RollingMeritCycleSegmentReportCountsEachRecordOnce();
    }
}
