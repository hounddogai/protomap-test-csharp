namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AccessBadgeManifestReportChecks
{
    public static void AccessBadgeManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705607998);
        var items = new List<AccessBadgeManifest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AccessBadgeManifest.NewAccessBadgeManifest($"access-badge-manifest-{index}", "sample"));
        }
        var report = AccessBadgeManifestReport.BuildAccessBadgeManifestReport("test", items, clock);
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
        AccessBadgeManifestReportCountsEachRecordOnce();
    }
}
