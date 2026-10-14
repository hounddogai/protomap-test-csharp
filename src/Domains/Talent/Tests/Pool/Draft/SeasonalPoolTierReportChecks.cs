namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SeasonalPoolTierReportChecks
{
    public static void SeasonalPoolTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704295732);
        var items = new List<SeasonalPoolTier>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalPoolTier.NewSeasonalPoolTier($"seasonal-pool-tier-{index}", "sample"));
        }
        var report = SeasonalPoolTierReport.BuildSeasonalPoolTierReport("test", items, clock);
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
        SeasonalPoolTierReportCountsEachRecordOnce();
    }
}
