namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LenientEngagementManifestReportChecks
{
    public static void LenientEngagementManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706354858);
        var items = new List<LenientEngagementManifest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientEngagementManifest.NewLenientEngagementManifest($"lenient-engagement-manifest-{index}", "sample"));
        }
        var report = LenientEngagementManifestReport.BuildLenientEngagementManifestReport("test", items, clock);
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
        LenientEngagementManifestReportCountsEachRecordOnce();
    }
}
