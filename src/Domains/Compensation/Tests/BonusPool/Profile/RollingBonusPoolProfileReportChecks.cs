namespace Acme.Workforce.Compensation.Tests.BonusPool.Profile;

using Acme.Workforce.Compensation.BonusPool.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RollingBonusPoolProfileReportChecks
{
    public static void RollingBonusPoolProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707325691);
        var items = new List<RollingBonusPoolProfile>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingBonusPoolProfile.NewRollingBonusPoolProfile($"rolling-bonus-pool-profile-{index}", "sample"));
        }
        var report = RollingBonusPoolProfileReport.BuildRollingBonusPoolProfileReport("test", items, clock);
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
        RollingBonusPoolProfileReportCountsEachRecordOnce();
    }
}
