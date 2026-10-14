namespace Acme.Workforce.Workplace.Tests.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.StrictBookingPlan;

/// <summary>Checks of strict booking plan code, run by the project's runner.</summary>
public static class BookingSlotReportChecks
{
    public static void BookingSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705655020);
        var items = new List<BookingSlot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BookingSlot.NewBookingSlot($"booking-slot-{index}", "sample"));
        }
        var report = BookingSlotReport.BuildBookingSlotReport("test", items, clock);
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
        BookingSlotReportCountsEachRecordOnce();
    }
}
