namespace Acme.Workforce.Orgdesign.Tests.CostModel.Assignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CostModelAssignmentReportChecks
{
    public static void CostModelAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705849685);
        var items = new List<CostModelAssignment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(CostModelAssignment.NewCostModelAssignment($"cost-model-assignment-{index}", "sample"));
        }
        var report = CostModelAssignmentReport.BuildCostModelAssignmentReport("test", items, clock);
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
        CostModelAssignmentReportCountsEachRecordOnce();
    }
}
