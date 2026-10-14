namespace Acme.Workforce.Talent.Tests.Nomination.Profile;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalNominationCycleReportChecks
{
    public static void SeasonalNominationCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701565976);
        var items = new List<SeasonalNominationCycle>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SeasonalNominationCycle.NewSeasonalNominationCycle($"seasonal-nomination-cycle-{index}", "sample"));
        }
        var report = SeasonalNominationCycleReport.BuildSeasonalNominationCycleReport("test", items, clock);
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
        SeasonalNominationCycleReportCountsEachRecordOnce();
    }
}
