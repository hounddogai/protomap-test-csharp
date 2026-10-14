namespace Acme.Workforce.Assets.Tests.Warranty.Milestone;

using Acme.Workforce.Assets.Warranty.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ManualWarrantyEstimateReportChecks
{
    public static void ManualWarrantyEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704874733);
        var items = new List<ManualWarrantyEstimate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ManualWarrantyEstimate.NewManualWarrantyEstimate($"manual-warranty-estimate-{index}", "sample"));
        }
        var report = ManualWarrantyEstimateReport.BuildManualWarrantyEstimateReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ManualWarrantyEstimateReportCountsEachRecordOnce();
    }
}
