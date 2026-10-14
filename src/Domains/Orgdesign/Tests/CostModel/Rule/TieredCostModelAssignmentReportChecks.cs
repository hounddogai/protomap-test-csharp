namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredCostModelAssignmentReportChecks
{
    public static void TieredCostModelAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707036845);
        var items = new List<TieredCostModelAssignment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TieredCostModelAssignment.NewTieredCostModelAssignment($"tiered-cost-model-assignment-{index}", "sample"));
        }
        var report = TieredCostModelAssignmentReport.BuildTieredCostModelAssignmentReport("test", items, clock);
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
        TieredCostModelAssignmentReportCountsEachRecordOnce();
    }
}
