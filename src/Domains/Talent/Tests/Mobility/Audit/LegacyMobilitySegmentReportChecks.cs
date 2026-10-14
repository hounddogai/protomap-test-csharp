namespace Acme.Workforce.Talent.Tests.Mobility.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyMobilitySegmentReportChecks
{
    public static void LegacyMobilitySegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702848136);
        var items = new List<LegacyMobilitySegment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyMobilitySegment.NewLegacyMobilitySegment($"legacy-mobility-segment-{index}", "sample"));
        }
        var report = LegacyMobilitySegmentReport.BuildLegacyMobilitySegmentReport("test", items, clock);
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
        LegacyMobilitySegmentReportCountsEachRecordOnce();
    }
}
