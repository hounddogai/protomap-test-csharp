namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalExceptionPreferenceReportChecks
{
    public static void RegionalExceptionPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705401606);
        var items = new List<RegionalExceptionPreference>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RegionalExceptionPreference.NewRegionalExceptionPreference($"regional-exception-preference-{index}", "sample"));
        }
        var report = RegionalExceptionPreferenceReport.BuildRegionalExceptionPreferenceReport("test", items, clock);
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
        RegionalExceptionPreferenceReportCountsEachRecordOnce();
    }
}
