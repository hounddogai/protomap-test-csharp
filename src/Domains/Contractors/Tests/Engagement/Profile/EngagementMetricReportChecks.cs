namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class EngagementMetricReportChecks
{
    public static void EngagementMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702006234);
        var items = new List<EngagementMetric>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(EngagementMetric.NewEngagementMetric($"engagement-metric-{index}", "sample"));
        }
        var report = EngagementMetricReport.BuildEngagementMetricReport("test", items, clock);
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
        EngagementMetricReportCountsEachRecordOnce();
    }
}
