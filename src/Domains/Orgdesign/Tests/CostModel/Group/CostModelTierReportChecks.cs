namespace Acme.Workforce.Orgdesign.Tests.CostModel.Group;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class CostModelTierReportChecks
{
    public static void CostModelTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701457793);
        var items = new List<CostModelTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CostModelTier.NewCostModelTier($"cost-model-tier-{index}", "sample"));
        }
        var report = CostModelTierReport.BuildCostModelTierReport("test", items, clock);
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
        CostModelTierReportCountsEachRecordOnce();
    }
}
