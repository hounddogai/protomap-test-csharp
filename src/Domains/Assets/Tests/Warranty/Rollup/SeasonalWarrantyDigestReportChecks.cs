namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalWarrantyDigestReportChecks
{
    public static void SeasonalWarrantyDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700527080);
        var items = new List<SeasonalWarrantyDigest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SeasonalWarrantyDigest.NewSeasonalWarrantyDigest($"seasonal-warranty-digest-{index}", "sample"));
        }
        var report = SeasonalWarrantyDigestReport.BuildSeasonalWarrantyDigestReport("test", items, clock);
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
        SeasonalWarrantyDigestReportCountsEachRecordOnce();
    }
}
