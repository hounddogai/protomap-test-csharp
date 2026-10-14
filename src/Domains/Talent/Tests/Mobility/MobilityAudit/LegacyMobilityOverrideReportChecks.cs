namespace Acme.Workforce.Talent.Tests.Mobility.MobilityAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>Checks of mobility audit code, run by the project's runner.</summary>
public static class LegacyMobilityOverrideReportChecks
{
    public static void LegacyMobilityOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705056457);
        var items = new List<LegacyMobilityOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyMobilityOverride.NewLegacyMobilityOverride($"legacy-mobility-override-{index}", "sample"));
        }
        var report = LegacyMobilityOverrideReport.BuildLegacyMobilityOverrideReport("test", items, clock);
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
        LegacyMobilityOverrideReportCountsEachRecordOnce();
    }
}
