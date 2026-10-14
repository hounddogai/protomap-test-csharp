namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdjustmentMappingReportChecks
{
    public static void AdjustmentMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702334438);
        var items = new List<AdjustmentMapping>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdjustmentMapping.NewAdjustmentMapping($"adjustment-mapping-{index}", "sample"));
        }
        var report = AdjustmentMappingReport.BuildAdjustmentMappingReport("test", items, clock);
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
        AdjustmentMappingReportCountsEachRecordOnce();
    }
}
