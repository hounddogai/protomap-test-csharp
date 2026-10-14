namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackEngagementExceptionReportChecks
{
    public static void FallbackEngagementExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706314286);
        var items = new List<FallbackEngagementException>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackEngagementException.NewFallbackEngagementException($"fallback-engagement-exception-{index}", "sample"));
        }
        var report = FallbackEngagementExceptionReport.BuildFallbackEngagementExceptionReport("test", items, clock);
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
        FallbackEngagementExceptionReportCountsEachRecordOnce();
    }
}
