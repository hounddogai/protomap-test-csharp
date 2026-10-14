namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PendingAssetReturnPlanReportChecks
{
    public static void PendingAssetReturnPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708018376);
        var items = new List<PendingAssetReturnPlan>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PendingAssetReturnPlan.NewPendingAssetReturnPlan($"pending-asset-return-plan-{index}", "sample"));
        }
        var report = PendingAssetReturnPlanReport.BuildPendingAssetReturnPlanReport("test", items, clock);
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
        PendingAssetReturnPlanReportCountsEachRecordOnce();
    }
}
