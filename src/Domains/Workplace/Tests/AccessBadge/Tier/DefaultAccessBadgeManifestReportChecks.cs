namespace Acme.Workforce.Workplace.Tests.AccessBadge.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class DefaultAccessBadgeManifestReportChecks
{
    public static void DefaultAccessBadgeManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707089231);
        var items = new List<DefaultAccessBadgeManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest($"default-access-badge-manifest-{index}", "sample"));
        }
        var report = DefaultAccessBadgeManifestReport.BuildDefaultAccessBadgeManifestReport("test", items, clock);
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
        DefaultAccessBadgeManifestReportCountsEachRecordOnce();
    }
}
