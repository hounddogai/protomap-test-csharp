namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LicenseSeatPreferenceReportChecks
{
    public static void LicenseSeatPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708719459);
        var items = new List<LicenseSeatPreference>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LicenseSeatPreference.NewLicenseSeatPreference($"license-seat-preference-{index}", "sample"));
        }
        var report = LicenseSeatPreferenceReport.BuildLicenseSeatPreferenceReport("test", items, clock);
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
        LicenseSeatPreferenceReportCountsEachRecordOnce();
    }
}
