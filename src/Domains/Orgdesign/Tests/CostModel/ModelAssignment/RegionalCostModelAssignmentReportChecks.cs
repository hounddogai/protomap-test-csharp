namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class RegionalCostModelAssignmentReportChecks
{
    public static void RegionalCostModelAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707887675);
        var items = new List<RegionalCostModelAssignment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RegionalCostModelAssignment.NewRegionalCostModelAssignment($"regional-cost-model-assignment-{index}", "sample"));
        }
        var report = RegionalCostModelAssignmentReport.BuildRegionalCostModelAssignmentReport("test", items, clock);
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
        RegionalCostModelAssignmentReportCountsEachRecordOnce();
    }
}
