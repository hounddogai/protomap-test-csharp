namespace Acme.Workforce.Assets.Tests.Warranty.Notice;

using Acme.Workforce.Assets.Warranty.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedWarrantyBaselineReportChecks
{
    public static void CappedWarrantyBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702694530);
        var items = new List<CappedWarrantyBaseline>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(CappedWarrantyBaseline.NewCappedWarrantyBaseline($"capped-warranty-baseline-{index}", "sample"));
        }
        var report = CappedWarrantyBaselineReport.BuildCappedWarrantyBaselineReport("test", items, clock);
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
        CappedWarrantyBaselineReportCountsEachRecordOnce();
    }
}
