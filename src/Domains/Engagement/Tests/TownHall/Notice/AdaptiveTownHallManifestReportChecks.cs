namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdaptiveTownHallManifestReportChecks
{
    public static void AdaptiveTownHallManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706056422);
        var items = new List<AdaptiveTownHallManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdaptiveTownHallManifest.NewAdaptiveTownHallManifest($"adaptive-town-hall-manifest-{index}", "sample"));
        }
        var report = AdaptiveTownHallManifestReport.BuildAdaptiveTownHallManifestReport("test", items, clock);
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
        AdaptiveTownHallManifestReportCountsEachRecordOnce();
    }
}
