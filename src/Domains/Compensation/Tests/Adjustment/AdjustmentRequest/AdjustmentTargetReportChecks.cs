namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class AdjustmentTargetReportChecks
{
    public static void AdjustmentTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709003431);
        var items = new List<AdjustmentTarget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdjustmentTarget.NewAdjustmentTarget($"adjustment-target-{index}", "sample"));
        }
        var report = AdjustmentTargetReport.BuildAdjustmentTargetReport("test", items, clock);
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
        AdjustmentTargetReportCountsEachRecordOnce();
    }
}
