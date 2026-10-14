namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class CappedCostModelRuleReportChecks
{
    public static void CappedCostModelRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704505226);
        var items = new List<CappedCostModelRule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedCostModelRule.NewCappedCostModelRule($"capped-cost-model-rule-{index}", "sample"));
        }
        var report = CappedCostModelRuleReport.BuildCappedCostModelRuleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CappedCostModelRuleReportCountsEachRecordOnce();
    }
}
