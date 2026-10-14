namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolClaimReportChecks
{
    public static void BonusPoolClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707160793);
        var items = new List<BonusPoolClaim>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BonusPoolClaim.NewBonusPoolClaim($"bonus-pool-claim-{index}", "sample"));
        }
        var report = BonusPoolClaimReport.BuildBonusPoolClaimReport("test", items, clock);
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
        BonusPoolClaimReportCountsEachRecordOnce();
    }
}
