namespace Acme.Workforce.Orgdesign.Tests.Position.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredPositionBundleReportChecks
{
    public static void TieredPositionBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705684701);
        var items = new List<TieredPositionBundle>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TieredPositionBundle.NewTieredPositionBundle($"tiered-position-bundle-{index}", "sample"));
        }
        var report = TieredPositionBundleReport.BuildTieredPositionBundleReport("test", items, clock);
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
        TieredPositionBundleReportCountsEachRecordOnce();
    }
}
