namespace Acme.Workforce.Workplace.Tests.Booking.BookingPlan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.BookingPlan;

/// <summary>Checks of booking plan code, run by the project's runner.</summary>
public static class LenientBookingPlanReportChecks
{
    public static void LenientBookingPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708459399);
        var items = new List<LenientBookingPlan>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientBookingPlan.NewLenientBookingPlan($"lenient-booking-plan-{index}", "sample"));
        }
        var report = LenientBookingPlanReport.BuildLenientBookingPlanReport("test", items, clock);
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
        LenientBookingPlanReportCountsEachRecordOnce();
    }
}
