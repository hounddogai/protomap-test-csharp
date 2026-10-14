namespace Acme.Workforce.Workplace.Tests.Booking.Plan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BookingPlanReportChecks
{
    public static void BookingPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701708306);
        var items = new List<BookingPlan>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BookingPlan.NewBookingPlan($"booking-plan-{index}", "sample"));
        }
        var report = BookingPlanReport.BuildBookingPlanReport("test", items, clock);
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
        BookingPlanReportCountsEachRecordOnce();
    }
}
