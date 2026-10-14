namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class TieredReturnPlanThresholdReportChecks
{
    public static void TieredReturnPlanThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708333760);
        var items = new List<TieredReturnPlanThreshold>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TieredReturnPlanThreshold.NewTieredReturnPlanThreshold($"tiered-return-plan-threshold-{index}", "sample"));
        }
        var report = TieredReturnPlanThresholdReport.BuildTieredReturnPlanThresholdReport("test", items, clock);
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
        TieredReturnPlanThresholdReportCountsEachRecordOnce();
    }
}
