namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class DefaultWarrantyCycleReportChecks
{
    public static void DefaultWarrantyCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703353446);
        var items = new List<DefaultWarrantyCycle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DefaultWarrantyCycle.NewDefaultWarrantyCycle($"default-warranty-cycle-{index}", "sample"));
        }
        var report = DefaultWarrantyCycleReport.BuildDefaultWarrantyCycleReport("test", items, clock);
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
        DefaultWarrantyCycleReportCountsEachRecordOnce();
    }
}
