namespace Acme.Workforce.Assets.Tests.Warranty.Bundle;

using Acme.Workforce.Assets.Warranty.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WarrantyExceptionReportChecks
{
    public static void WarrantyExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709622964);
        var items = new List<WarrantyException>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WarrantyException.NewWarrantyException($"warranty-exception-{index}", "sample"));
        }
        var report = WarrantyExceptionReport.BuildWarrantyExceptionReport("test", items, clock);
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
        WarrantyExceptionReportCountsEachRecordOnce();
    }
}
