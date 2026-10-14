namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class RollingPositionBundleReportChecks
{
    public static void RollingPositionBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708389217);
        var items = new List<RollingPositionBundle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RollingPositionBundle.NewRollingPositionBundle($"rolling-position-bundle-{index}", "sample"));
        }
        var report = RollingPositionBundleReport.BuildRollingPositionBundleReport("test", items, clock);
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
        RollingPositionBundleReportCountsEachRecordOnce();
    }
}
