namespace Acme.Workforce.Orgdesign.Tests.Position.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PositionThresholdReportChecks
{
    public static void PositionThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701721718);
        var items = new List<PositionThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PositionThreshold.NewPositionThreshold($"position-threshold-{index}", "sample"));
        }
        var report = PositionThresholdReport.BuildPositionThresholdReport("test", items, clock);
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
        PositionThresholdReportCountsEachRecordOnce();
    }
}
