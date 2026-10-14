namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class SeasonalPoolOverrideReportChecks
{
    public static void SeasonalPoolOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708082806);
        var items = new List<SeasonalPoolOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalPoolOverride.NewSeasonalPoolOverride($"seasonal-pool-override-{index}", "sample"));
        }
        var report = SeasonalPoolOverrideReport.BuildSeasonalPoolOverrideReport("test", items, clock);
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
        SeasonalPoolOverrideReportCountsEachRecordOnce();
    }
}
