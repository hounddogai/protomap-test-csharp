namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class LegacyCostModelGroupReportChecks
{
    public static void LegacyCostModelGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700772968);
        var items = new List<LegacyCostModelGroup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyCostModelGroup.NewLegacyCostModelGroup($"legacy-cost-model-group-{index}", "sample"));
        }
        var report = LegacyCostModelGroupReport.BuildLegacyCostModelGroupReport("test", items, clock);
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
        LegacyCostModelGroupReportCountsEachRecordOnce();
    }
}
