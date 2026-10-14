namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelOverrideReportChecks
{
    public static void CostModelOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709899692);
        var items = new List<CostModelOverride>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CostModelOverride.NewCostModelOverride($"cost-model-override-{index}", "sample"));
        }
        var report = CostModelOverrideReport.BuildCostModelOverrideReport("test", items, clock);
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
        CostModelOverrideReportCountsEachRecordOnce();
    }
}
