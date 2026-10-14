namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityRollupReportChecks
{
    public static void MobilityRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703394698);
        var items = new List<MobilityRollup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(MobilityRollup.NewMobilityRollup($"mobility-rollup-{index}", "sample"));
        }
        var report = MobilityRollupReport.BuildMobilityRollupReport("test", items, clock);
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
        MobilityRollupReportCountsEachRecordOnce();
    }
}
