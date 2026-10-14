namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantyGroupReportChecks
{
    public static void WarrantyGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703632327);
        var items = new List<WarrantyGroup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(WarrantyGroup.NewWarrantyGroup($"warranty-group-{index}", "sample"));
        }
        var report = WarrantyGroupReport.BuildWarrantyGroupReport("test", items, clock);
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
        WarrantyGroupReportCountsEachRecordOnce();
    }
}
