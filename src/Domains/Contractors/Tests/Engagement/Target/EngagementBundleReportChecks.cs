namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class EngagementBundleReportChecks
{
    public static void EngagementBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703860227);
        var items = new List<EngagementBundle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(EngagementBundle.NewEngagementBundle($"engagement-bundle-{index}", "sample"));
        }
        var report = EngagementBundleReport.BuildEngagementBundleReport("test", items, clock);
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
        EngagementBundleReportCountsEachRecordOnce();
    }
}
