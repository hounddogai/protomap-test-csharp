namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LegacyAssetReturnPlanReportChecks
{
    public static void LegacyAssetReturnPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708414834);
        var items = new List<LegacyAssetReturnPlan>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LegacyAssetReturnPlan.NewLegacyAssetReturnPlan($"legacy-asset-return-plan-{index}", "sample"));
        }
        var report = LegacyAssetReturnPlanReport.BuildLegacyAssetReturnPlanReport("test", items, clock);
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
        LegacyAssetReturnPlanReportCountsEachRecordOnce();
    }
}
