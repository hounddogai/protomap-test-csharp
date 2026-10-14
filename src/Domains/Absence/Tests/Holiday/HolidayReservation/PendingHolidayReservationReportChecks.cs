namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class PendingHolidayReservationReportChecks
{
    public static void PendingHolidayReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705379664);
        var items = new List<PendingHolidayReservation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PendingHolidayReservation.NewPendingHolidayReservation($"pending-holiday-reservation-{index}", "sample"));
        }
        var report = PendingHolidayReservationReport.BuildPendingHolidayReservationReport("test", items, clock);
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
        PendingHolidayReservationReportCountsEachRecordOnce();
    }
}
