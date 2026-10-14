namespace Acme.Workforce.Payrollops.Tests.PayRun.Preference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedPayRunPreferenceReportChecks
{
    public static void WeightedPayRunPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700652756);
        var items = new List<WeightedPayRunPreference>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(WeightedPayRunPreference.NewWeightedPayRunPreference($"weighted-pay-run-preference-{index}", "sample"));
        }
        var report = WeightedPayRunPreferenceReport.BuildWeightedPayRunPreferenceReport("test", items, clock);
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
        WeightedPayRunPreferenceReportCountsEachRecordOnce();
    }
}
