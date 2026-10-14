namespace Acme.Workforce.Compensation.Tests.BonusPool.Profile;

using Acme.Workforce.Compensation.BonusPool.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DefaultBonusPoolReservationReportChecks
{
    public static void DefaultBonusPoolReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707409069);
        var items = new List<DefaultBonusPoolReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DefaultBonusPoolReservation.NewDefaultBonusPoolReservation($"default-bonus-pool-reservation-{index}", "sample"));
        }
        var report = DefaultBonusPoolReservationReport.BuildDefaultBonusPoolReservationReport("test", items, clock);
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
        DefaultBonusPoolReservationReportCountsEachRecordOnce();
    }
}
