namespace Acme.Workforce.Orgdesign.Tests.Span.Exception;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class SpanReservationReportChecks
{
    public static void SpanReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709325574);
        var items = new List<SpanReservation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SpanReservation.NewSpanReservation($"span-reservation-{index}", "sample"));
        }
        var report = SpanReservationReport.BuildSpanReservationReport("test", items, clock);
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
        SpanReservationReportCountsEachRecordOnce();
    }
}
