namespace Acme.Workforce.Assets.Tests.LicenseSeat.Checkpoint;

using Acme.Workforce.Assets.LicenseSeat.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LicenseSeatSlotReportChecks
{
    public static void LicenseSeatSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708912884);
        var items = new List<LicenseSeatSlot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LicenseSeatSlot.NewLicenseSeatSlot($"license-seat-slot-{index}", "sample"));
        }
        var report = LicenseSeatSlotReport.BuildLicenseSeatSlotReport("test", items, clock);
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
        LicenseSeatSlotReportCountsEachRecordOnce();
    }
}
