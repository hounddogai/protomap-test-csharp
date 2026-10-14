namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class WeightedTownHallManifestReportChecks
{
    public static void WeightedTownHallManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704024344);
        var items = new List<WeightedTownHallManifest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedTownHallManifest.NewWeightedTownHallManifest($"weighted-town-hall-manifest-{index}", "sample"));
        }
        var report = WeightedTownHallManifestReport.BuildWeightedTownHallManifestReport("test", items, clock);
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
        WeightedTownHallManifestReportCountsEachRecordOnce();
    }
}
