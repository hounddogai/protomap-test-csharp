namespace Acme.Workforce.Talent.Tests.CareerPath.Segment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CareerPathSegmentReportChecks
{
    public static void CareerPathSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703529652);
        var items = new List<CareerPathSegment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(CareerPathSegment.NewCareerPathSegment($"career-path-segment-{index}", "sample"));
        }
        var report = CareerPathSegmentReport.BuildCareerPathSegmentReport("test", items, clock);
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
        CareerPathSegmentReportCountsEachRecordOnce();
    }
}
