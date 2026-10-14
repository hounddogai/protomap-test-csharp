namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleSegmentReportChecks
{
    public static void MeritCycleSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702475993);
        var items = new List<MeritCycleSegment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(MeritCycleSegment.NewMeritCycleSegment($"merit-cycle-segment-{index}", "sample"));
        }
        var report = MeritCycleSegmentReport.BuildMeritCycleSegmentReport("test", items, clock);
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
        MeritCycleSegmentReportCountsEachRecordOnce();
    }
}
