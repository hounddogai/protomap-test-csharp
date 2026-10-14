namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class AdaptiveCostModelReservationReportChecks
{
    public static void AdaptiveCostModelReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706392922);
        var items = new List<AdaptiveCostModelReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptiveCostModelReservation.NewAdaptiveCostModelReservation($"adaptive-cost-model-reservation-{index}", "sample"));
        }
        var report = AdaptiveCostModelReservationReport.BuildAdaptiveCostModelReservationReport("test", items, clock);
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
        AdaptiveCostModelReservationReportCountsEachRecordOnce();
    }
}
