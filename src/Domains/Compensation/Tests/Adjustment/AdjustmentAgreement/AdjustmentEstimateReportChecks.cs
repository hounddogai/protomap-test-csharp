namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment agreement code, run by the project's runner.</summary>
public static class AdjustmentEstimateReportChecks
{
    public static void AdjustmentEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704230411);
        var items = new List<AdjustmentEstimate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdjustmentEstimate.NewAdjustmentEstimate($"adjustment-estimate-{index}", "sample"));
        }
        var report = AdjustmentEstimateReport.BuildAdjustmentEstimateReport("test", items, clock);
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
        AdjustmentEstimateReportCountsEachRecordOnce();
    }
}
