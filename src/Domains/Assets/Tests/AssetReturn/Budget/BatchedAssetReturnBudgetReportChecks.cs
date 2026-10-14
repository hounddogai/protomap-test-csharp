namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedAssetReturnBudgetReportChecks
{
    public static void BatchedAssetReturnBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700347745);
        var items = new List<BatchedAssetReturnBudget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BatchedAssetReturnBudget.NewBatchedAssetReturnBudget($"batched-asset-return-budget-{index}", "sample"));
        }
        var report = BatchedAssetReturnBudgetReport.BuildBatchedAssetReturnBudgetReport("test", items, clock);
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
        BatchedAssetReturnBudgetReportCountsEachRecordOnce();
    }
}
