namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class DefaultEngagementDigestReportChecks
{
    public static void DefaultEngagementDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705964837);
        var items = new List<DefaultEngagementDigest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(DefaultEngagementDigest.NewDefaultEngagementDigest($"default-engagement-digest-{index}", "sample"));
        }
        var report = DefaultEngagementDigestReport.BuildDefaultEngagementDigestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DefaultEngagementDigestReportCountsEachRecordOnce();
    }
}
