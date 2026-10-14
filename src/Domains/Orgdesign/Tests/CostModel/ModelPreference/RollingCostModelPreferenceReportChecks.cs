namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPreference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>Checks of model preference code, run by the project's runner.</summary>
public static class RollingCostModelPreferenceReportChecks
{
    public static void RollingCostModelPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701926276);
        var items = new List<RollingCostModelPreference>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RollingCostModelPreference.NewRollingCostModelPreference($"rolling-cost-model-preference-{index}", "sample"));
        }
        var report = RollingCostModelPreferenceReport.BuildRollingCostModelPreferenceReport("test", items, clock);
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
        RollingCostModelPreferenceReportCountsEachRecordOnce();
    }
}
