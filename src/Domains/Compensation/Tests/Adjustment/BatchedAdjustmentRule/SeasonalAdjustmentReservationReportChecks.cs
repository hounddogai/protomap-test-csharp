namespace Acme.Workforce.Compensation.Tests.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batched adjustment rule code, run by the project's runner.</summary>
public static class SeasonalAdjustmentReservationReportChecks
{
    public static void SeasonalAdjustmentReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708823952);
        var items = new List<SeasonalAdjustmentReservation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalAdjustmentReservation.NewSeasonalAdjustmentReservation($"seasonal-adjustment-reservation-{index}", "sample"));
        }
        var report = SeasonalAdjustmentReservationReport.BuildSeasonalAdjustmentReservationReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentReservationReportCountsEachRecordOnce();
    }
}
