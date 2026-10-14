namespace Acme.Workforce.Compensation.Tests.BonusPool.Exception;

using Acme.Workforce.Compensation.BonusPool.Exception;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class FallbackBonusPoolProfileReportChecks
{
    public static void FallbackBonusPoolProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709377518);
        var items = new List<FallbackBonusPoolProfile>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FallbackBonusPoolProfile.NewFallbackBonusPoolProfile($"fallback-bonus-pool-profile-{index}", "sample"));
        }
        var report = FallbackBonusPoolProfileReport.BuildFallbackBonusPoolProfileReport("test", items, clock);
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
        FallbackBonusPoolProfileReportCountsEachRecordOnce();
    }
}
