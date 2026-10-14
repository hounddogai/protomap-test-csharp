namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class LenientReorgSlotReportChecks
{
    public static void LenientReorgSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703645535);
        var items = new List<LenientReorgSlot>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LenientReorgSlot.NewLenientReorgSlot($"lenient-reorg-slot-{index}", "sample"));
        }
        var report = LenientReorgSlotReport.BuildLenientReorgSlotReport("test", items, clock);
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
        LenientReorgSlotReportCountsEachRecordOnce();
    }
}
