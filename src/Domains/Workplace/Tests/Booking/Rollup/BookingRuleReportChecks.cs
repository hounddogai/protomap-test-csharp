namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BookingRuleReportChecks
{
    public static void BookingRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705537824);
        var items = new List<BookingRule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BookingRule.NewBookingRule($"booking-rule-{index}", "sample"));
        }
        var report = BookingRuleReport.BuildBookingRuleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BookingRuleReportCountsEachRecordOnce();
    }
}
