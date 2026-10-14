namespace Acme.Workforce.Talent.Tests.Nomination.Segment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class NominationSegmentReportChecks
{
    public static void NominationSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702301349);
        var items = new List<NominationSegment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(NominationSegment.NewNominationSegment($"nomination-segment-{index}", "sample"));
        }
        var report = NominationSegmentReport.BuildNominationSegmentReport("test", items, clock);
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
        NominationSegmentReportCountsEachRecordOnce();
    }
}
