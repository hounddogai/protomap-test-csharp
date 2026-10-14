namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanPreferenceReportChecks
{
    public static void SeasonalSuccessionPlanPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702585258);
        var items = new List<SeasonalSuccessionPlanPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalSuccessionPlanPreference.NewSeasonalSuccessionPlanPreference($"seasonal-succession-plan-preference-{index}", "sample"));
        }
        var report = SeasonalSuccessionPlanPreferenceReport.BuildSeasonalSuccessionPlanPreferenceReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SeasonalSuccessionPlanPreferenceReportCountsEachRecordOnce();
    }
}
