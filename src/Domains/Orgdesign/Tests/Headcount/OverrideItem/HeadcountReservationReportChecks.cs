namespace Acme.Workforce.Orgdesign.Tests.Headcount.OverrideItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HeadcountReservationReportChecks
{
    public static void HeadcountReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709541431);
        var items = new List<HeadcountReservation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(HeadcountReservation.NewHeadcountReservation($"headcount-reservation-{index}", "sample"));
        }
        var report = HeadcountReservationReport.BuildHeadcountReservationReport("test", items, clock);
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
        HeadcountReservationReportCountsEachRecordOnce();
    }
}
