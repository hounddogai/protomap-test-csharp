namespace Acme.Workforce.Payrollops.Tests.Garnishment.Preference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class GarnishmentPreferenceReportChecks
{
    public static void GarnishmentPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709994106);
        var items = new List<GarnishmentPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(GarnishmentPreference.NewGarnishmentPreference($"garnishment-preference-{index}", "sample"));
        }
        var report = GarnishmentPreferenceReport.BuildGarnishmentPreferenceReport("test", items, clock);
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
        GarnishmentPreferenceReportCountsEachRecordOnce();
    }
}
