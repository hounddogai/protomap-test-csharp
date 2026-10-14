namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class PrimaryBonusPoolTierReportChecks
{
    public static void PrimaryBonusPoolTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703387306);
        var items = new List<PrimaryBonusPoolTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PrimaryBonusPoolTier.NewPrimaryBonusPoolTier($"primary-bonus-pool-tier-{index}", "sample"));
        }
        var report = PrimaryBonusPoolTierReport.BuildPrimaryBonusPoolTierReport("test", items, clock);
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
        PrimaryBonusPoolTierReportCountsEachRecordOnce();
    }
}
