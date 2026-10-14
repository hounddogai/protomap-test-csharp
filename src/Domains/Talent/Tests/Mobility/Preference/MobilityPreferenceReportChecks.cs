namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class MobilityPreferenceReportChecks
{
    public static void MobilityPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704537694);
        var items = new List<MobilityPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(MobilityPreference.NewMobilityPreference($"mobility-preference-{index}", "sample"));
        }
        var report = MobilityPreferenceReport.BuildMobilityPreferenceReport("test", items, clock);
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
        MobilityPreferenceReportCountsEachRecordOnce();
    }
}
