namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AdjustmentVarianceReportChecks
{
    public static void AdjustmentVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709121412);
        var items = new List<AdjustmentVariance>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdjustmentVariance.NewAdjustmentVariance($"adjustment-variance-{index}", "sample"));
        }
        var report = AdjustmentVarianceReport.BuildAdjustmentVarianceReport("test", items, clock);
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
        AdjustmentVarianceReportCountsEachRecordOnce();
    }
}
