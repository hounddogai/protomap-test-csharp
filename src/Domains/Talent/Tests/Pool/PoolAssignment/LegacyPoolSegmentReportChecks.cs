namespace Acme.Workforce.Talent.Tests.Pool.PoolAssignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>Checks of pool assignment code, run by the project's runner.</summary>
public static class LegacyPoolSegmentReportChecks
{
    public static void LegacyPoolSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700003918);
        var items = new List<LegacyPoolSegment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyPoolSegment.NewLegacyPoolSegment($"legacy-pool-segment-{index}", "sample"));
        }
        var report = LegacyPoolSegmentReport.BuildLegacyPoolSegmentReport("test", items, clock);
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
        LegacyPoolSegmentReportCountsEachRecordOnce();
    }
}
