namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class CostModelGroupReportChecks
{
    public static void CostModelGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704309280);
        var items = new List<CostModelGroup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CostModelGroup.NewCostModelGroup($"cost-model-group-{index}", "sample"));
        }
        var report = CostModelGroupReport.BuildCostModelGroupReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CostModelGroupReportCountsEachRecordOnce();
    }
}
