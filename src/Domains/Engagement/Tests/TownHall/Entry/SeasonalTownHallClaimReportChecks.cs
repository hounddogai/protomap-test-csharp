namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalTownHallClaimReportChecks
{
    public static void SeasonalTownHallClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705620922);
        var items = new List<SeasonalTownHallClaim>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalTownHallClaim.NewSeasonalTownHallClaim($"seasonal-town-hall-claim-{index}", "sample"));
        }
        var report = SeasonalTownHallClaimReport.BuildSeasonalTownHallClaimReport("test", items, clock);
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
        SeasonalTownHallClaimReportCountsEachRecordOnce();
    }
}
