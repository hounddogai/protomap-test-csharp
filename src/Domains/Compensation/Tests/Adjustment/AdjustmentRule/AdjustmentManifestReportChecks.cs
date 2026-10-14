namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class AdjustmentManifestReportChecks
{
    public static void AdjustmentManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705131905);
        var items = new List<AdjustmentManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdjustmentManifest.NewAdjustmentManifest($"adjustment-manifest-{index}", "sample"));
        }
        var report = AdjustmentManifestReport.BuildAdjustmentManifestReport("test", items, clock);
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
        AdjustmentManifestReportCountsEachRecordOnce();
    }
}
