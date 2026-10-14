namespace Acme.Workforce.Compensation.Tests.Adjustment.Rollup;

using Acme.Workforce.Compensation.Adjustment.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RegionalAdjustmentReservationReportChecks
{
    public static void RegionalAdjustmentReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705567042);
        var items = new List<RegionalAdjustmentReservation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RegionalAdjustmentReservation.NewRegionalAdjustmentReservation($"regional-adjustment-reservation-{index}", "sample"));
        }
        var report = RegionalAdjustmentReservationReport.BuildRegionalAdjustmentReservationReport("test", items, clock);
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
        RegionalAdjustmentReservationReportCountsEachRecordOnce();
    }
}
