namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SeasonalCostModelPreferenceReportChecks
{
    public static void SeasonalCostModelPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702497393);
        var items = new List<SeasonalCostModelPreference>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SeasonalCostModelPreference.NewSeasonalCostModelPreference($"seasonal-cost-model-preference-{index}", "sample"));
        }
        var report = SeasonalCostModelPreferenceReport.BuildSeasonalCostModelPreferenceReport("test", items, clock);
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
        SeasonalCostModelPreferenceReportCountsEachRecordOnce();
    }
}
