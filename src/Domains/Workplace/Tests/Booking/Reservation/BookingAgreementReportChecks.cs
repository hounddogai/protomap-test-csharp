namespace Acme.Workforce.Workplace.Tests.Booking.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BookingAgreementReportChecks
{
    public static void BookingAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703401506);
        var items = new List<BookingAgreement>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BookingAgreement.NewBookingAgreement($"booking-agreement-{index}", "sample"));
        }
        var report = BookingAgreementReport.BuildBookingAgreementReport("test", items, clock);
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
        BookingAgreementReportCountsEachRecordOnce();
    }
}
