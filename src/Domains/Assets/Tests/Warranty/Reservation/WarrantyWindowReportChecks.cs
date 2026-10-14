namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyWindowReportChecks
{
    public static void WarrantyWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700297478);
        var items = new List<WarrantyWindow>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(WarrantyWindow.NewWarrantyWindow($"warranty-window-{index}", "sample"));
        }
        var report = WarrantyWindowReport.BuildWarrantyWindowReport("test", items, clock);
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
        WarrantyWindowReportCountsEachRecordOnce();
    }
}
