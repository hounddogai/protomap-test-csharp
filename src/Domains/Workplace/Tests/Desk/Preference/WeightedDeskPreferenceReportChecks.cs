namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedDeskPreferenceReportChecks
{
    public static void WeightedDeskPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705323237);
        var items = new List<WeightedDeskPreference>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedDeskPreference.NewWeightedDeskPreference($"weighted-desk-preference-{index}", "sample"));
        }
        var report = WeightedDeskPreferenceReport.BuildWeightedDeskPreferenceReport("test", items, clock);
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
        WeightedDeskPreferenceReportCountsEachRecordOnce();
    }
}
