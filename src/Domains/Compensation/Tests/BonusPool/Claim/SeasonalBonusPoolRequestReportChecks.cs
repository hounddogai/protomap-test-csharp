namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SeasonalBonusPoolRequestReportChecks
{
    public static void SeasonalBonusPoolRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700870958);
        var items = new List<SeasonalBonusPoolRequest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SeasonalBonusPoolRequest.NewSeasonalBonusPoolRequest($"seasonal-bonus-pool-request-{index}", "sample"));
        }
        var report = SeasonalBonusPoolRequestReport.BuildSeasonalBonusPoolRequestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SeasonalBonusPoolRequestReportCountsEachRecordOnce();
    }
}
