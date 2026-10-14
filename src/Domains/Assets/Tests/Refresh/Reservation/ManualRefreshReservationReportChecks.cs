namespace Acme.Workforce.Assets.Tests.Refresh.Reservation;

using Acme.Workforce.Assets.Refresh.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ManualRefreshReservationReportChecks
{
    public static void ManualRefreshReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705079428);
        var items = new List<ManualRefreshReservation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ManualRefreshReservation.NewManualRefreshReservation($"manual-refresh-reservation-{index}", "sample"));
        }
        var report = ManualRefreshReservationReport.BuildManualRefreshReservationReport("test", items, clock);
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
        ManualRefreshReservationReportCountsEachRecordOnce();
    }
}
