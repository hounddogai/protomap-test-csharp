namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RollingEngagementDigestReportChecks
{
    public static void RollingEngagementDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703087096);
        var items = new List<RollingEngagementDigest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RollingEngagementDigest.NewRollingEngagementDigest($"rolling-engagement-digest-{index}", "sample"));
        }
        var report = RollingEngagementDigestReport.BuildRollingEngagementDigestReport("test", items, clock);
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
        RollingEngagementDigestReportCountsEachRecordOnce();
    }
}
