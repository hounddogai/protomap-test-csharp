namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class PositionVarianceReportChecks
{
    public static void PositionVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704982122);
        var items = new List<PositionVariance>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PositionVariance.NewPositionVariance($"position-variance-{index}", "sample"));
        }
        var report = PositionVarianceReport.BuildPositionVarianceReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PositionVarianceReportCountsEachRecordOnce();
    }
}
