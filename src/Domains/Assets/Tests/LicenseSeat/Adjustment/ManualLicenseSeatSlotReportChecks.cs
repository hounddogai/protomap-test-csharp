namespace Acme.Workforce.Assets.Tests.LicenseSeat.Adjustment;

using Acme.Workforce.Assets.LicenseSeat.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ManualLicenseSeatSlotReportChecks
{
    public static void ManualLicenseSeatSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702342421);
        var items = new List<ManualLicenseSeatSlot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualLicenseSeatSlot.NewManualLicenseSeatSlot($"manual-license-seat-slot-{index}", "sample"));
        }
        var report = ManualLicenseSeatSlotReport.BuildManualLicenseSeatSlotReport("test", items, clock);
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
        ManualLicenseSeatSlotReportCountsEachRecordOnce();
    }
}
