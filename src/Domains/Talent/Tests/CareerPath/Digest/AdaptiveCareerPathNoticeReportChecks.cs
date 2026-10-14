namespace Acme.Workforce.Talent.Tests.CareerPath.Digest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AdaptiveCareerPathNoticeReportChecks
{
    public static void AdaptiveCareerPathNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709217429);
        var items = new List<AdaptiveCareerPathNotice>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptiveCareerPathNotice.NewAdaptiveCareerPathNotice($"adaptive-career-path-notice-{index}", "sample"));
        }
        var report = AdaptiveCareerPathNoticeReport.BuildAdaptiveCareerPathNoticeReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdaptiveCareerPathNoticeReportCountsEachRecordOnce();
    }
}
