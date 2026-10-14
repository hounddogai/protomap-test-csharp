namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class FallbackPositionRecordReportChecks
{
    public static void FallbackPositionRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701637138);
        var items = new List<FallbackPositionRecord>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FallbackPositionRecord.NewFallbackPositionRecord($"fallback-position-record-{index}", "sample"));
        }
        var report = FallbackPositionRecordReport.BuildFallbackPositionRecordReport("test", items, clock);
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
        FallbackPositionRecordReportCountsEachRecordOnce();
    }
}
