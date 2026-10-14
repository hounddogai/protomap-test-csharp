namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantyRequestReportChecks
{
    public static void WarrantyRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705255454);
        var items = new List<WarrantyRequest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WarrantyRequest.NewWarrantyRequest($"warranty-request-{index}", "sample"));
        }
        var report = WarrantyRequestReport.BuildWarrantyRequestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WarrantyRequestReportCountsEachRecordOnce();
    }
}
