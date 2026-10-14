namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CappedAssetReturnPlanReportChecks
{
    public static void CappedAssetReturnPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707504399);
        var items = new List<CappedAssetReturnPlan>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(CappedAssetReturnPlan.NewCappedAssetReturnPlan($"capped-asset-return-plan-{index}", "sample"));
        }
        var report = CappedAssetReturnPlanReport.BuildCappedAssetReturnPlanReport("test", items, clock);
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
        CappedAssetReturnPlanReportCountsEachRecordOnce();
    }
}
