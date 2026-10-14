namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilityReviewReportChecks
{
    public static void MobilityReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700252583);
        var items = new List<MobilityReview>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(MobilityReview.NewMobilityReview($"mobility-review-{index}", "sample"));
        }
        var report = MobilityReviewReport.BuildMobilityReviewReport("test", items, clock);
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
        MobilityReviewReportCountsEachRecordOnce();
    }
}
