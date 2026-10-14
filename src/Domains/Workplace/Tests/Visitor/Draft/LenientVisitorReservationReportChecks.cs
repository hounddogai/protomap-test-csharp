namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientVisitorReservationReportChecks
{
    public static void LenientVisitorReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701048177);
        var items = new List<LenientVisitorReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientVisitorReservation.NewLenientVisitorReservation($"lenient-visitor-reservation-{index}", "sample"));
        }
        var report = LenientVisitorReservationReport.BuildLenientVisitorReservationReport("test", items, clock);
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
        LenientVisitorReservationReportCountsEachRecordOnce();
    }
}
