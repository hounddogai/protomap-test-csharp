namespace Acme.Workforce.Talent.Tests.Mobility.OverrideItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictMobilityOverrideReportChecks
{
    public static void StrictMobilityOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708165610);
        var items = new List<StrictMobilityOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StrictMobilityOverride.NewStrictMobilityOverride($"strict-mobility-override-{index}", "sample"));
        }
        var report = StrictMobilityOverrideReport.BuildStrictMobilityOverrideReport("test", items, clock);
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
        StrictMobilityOverrideReportCountsEachRecordOnce();
    }
}
