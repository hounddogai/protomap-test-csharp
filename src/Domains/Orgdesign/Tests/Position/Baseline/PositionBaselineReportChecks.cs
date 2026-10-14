namespace Acme.Workforce.Orgdesign.Tests.Position.Baseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PositionBaselineReportChecks
{
    public static void PositionBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709197149);
        var items = new List<PositionBaseline>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PositionBaseline.NewPositionBaseline($"position-baseline-{index}", "sample"));
        }
        var report = PositionBaselineReport.BuildPositionBaselineReport("test", items, clock);
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
        PositionBaselineReportCountsEachRecordOnce();
    }
}
