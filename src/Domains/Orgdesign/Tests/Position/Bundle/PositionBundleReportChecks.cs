namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PositionBundleReportChecks
{
    public static void PositionBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705206271);
        var items = new List<PositionBundle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PositionBundle.NewPositionBundle($"position-bundle-{index}", "sample"));
        }
        var report = PositionBundleReport.BuildPositionBundleReport("test", items, clock);
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
        PositionBundleReportCountsEachRecordOnce();
    }
}
