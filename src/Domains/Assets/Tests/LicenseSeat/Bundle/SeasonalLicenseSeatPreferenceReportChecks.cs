namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatPreferenceReportChecks
{
    public static void SeasonalLicenseSeatPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707668243);
        var items = new List<SeasonalLicenseSeatPreference>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalLicenseSeatPreference.NewSeasonalLicenseSeatPreference($"seasonal-license-seat-preference-{index}", "sample"));
        }
        var report = SeasonalLicenseSeatPreferenceReport.BuildSeasonalLicenseSeatPreferenceReport("test", items, clock);
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
        SeasonalLicenseSeatPreferenceReportCountsEachRecordOnce();
    }
}
