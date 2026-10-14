namespace Acme.Workforce.Assets.Tests.Warranty.Adjustment;

using Acme.Workforce.Assets.Warranty.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyWarrantyBaselineReportChecks
{
    public static void LegacyWarrantyBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705164747);
        var items = new List<LegacyWarrantyBaseline>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LegacyWarrantyBaseline.NewLegacyWarrantyBaseline($"legacy-warranty-baseline-{index}", "sample"));
        }
        var report = LegacyWarrantyBaselineReport.BuildLegacyWarrantyBaselineReport("test", items, clock);
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
        LegacyWarrantyBaselineReportCountsEachRecordOnce();
    }
}
