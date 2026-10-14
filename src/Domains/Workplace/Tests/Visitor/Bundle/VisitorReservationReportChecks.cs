namespace Acme.Workforce.Workplace.Tests.Visitor.Bundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class VisitorReservationReportChecks
{
    public static void VisitorReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709485556);
        var items = new List<VisitorReservation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(VisitorReservation.NewVisitorReservation($"visitor-reservation-{index}", "sample"));
        }
        var report = VisitorReservationReport.BuildVisitorReservationReport("test", items, clock);
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
        VisitorReservationReportCountsEachRecordOnce();
    }
}
