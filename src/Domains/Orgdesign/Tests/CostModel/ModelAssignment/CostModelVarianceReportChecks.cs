namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelVarianceReportChecks
{
    public static void CostModelVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700295580);
        var items = new List<CostModelVariance>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(CostModelVariance.NewCostModelVariance($"cost-model-variance-{index}", "sample"));
        }
        var report = CostModelVarianceReport.BuildCostModelVarianceReport("test", items, clock);
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
        CostModelVarianceReportCountsEachRecordOnce();
    }
}
