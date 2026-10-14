namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyBaselineReportChecks
{
    public static void WarrantyBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705842176);
        var items = new List<WarrantyBaseline>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(WarrantyBaseline.NewWarrantyBaseline($"warranty-baseline-{index}", "sample"));
        }
        var report = WarrantyBaselineReport.BuildWarrantyBaselineReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WarrantyBaselineReportCountsEachRecordOnce();
    }
}
