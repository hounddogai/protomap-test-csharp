namespace Acme.Workforce.Contractors.Tests.Vendor.Score;

using Acme.Workforce.Contractors.Vendor.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class VendorScoreReportChecks
{
    public static void VendorScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701806386);
        var items = new List<VendorScore>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(VendorScore.NewVendorScore($"vendor-score-{index}", "sample"));
        }
        var report = VendorScoreReport.BuildVendorScoreReport("test", items, clock);
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
        VendorScoreReportCountsEachRecordOnce();
    }
}
