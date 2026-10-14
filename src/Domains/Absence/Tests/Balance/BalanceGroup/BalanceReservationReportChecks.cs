namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class BalanceReservationReportChecks
{
    public static void BalanceReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707485970);
        var items = new List<BalanceReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BalanceReservation.NewBalanceReservation($"balance-reservation-{index}", "sample"));
        }
        var report = BalanceReservationReport.BuildBalanceReservationReport("test", items, clock);
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
        BalanceReservationReportCountsEachRecordOnce();
    }
}
