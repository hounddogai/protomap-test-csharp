namespace Acme.Workforce.Assets.Tests.AssetReturn.Rule;

using Acme.Workforce.Assets.AssetReturn.Rule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssetReturnRuleReportChecks
{
    public static void AssetReturnRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704160899);
        var items = new List<AssetReturnRule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AssetReturnRule.NewAssetReturnRule($"asset-return-rule-{index}", "sample"));
        }
        var report = AssetReturnRuleReport.BuildAssetReturnRuleReport("test", items, clock);
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
        AssetReturnRuleReportCountsEachRecordOnce();
    }
}
