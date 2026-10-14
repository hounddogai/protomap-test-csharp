namespace Acme.Workforce.Payrollops.Tests.RetroPay.Bundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RetroPayBundleReportChecks
{
    public static void RetroPayBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702464041);
        var items = new List<RetroPayBundle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RetroPayBundle.NewRetroPayBundle($"retro-pay-bundle-{index}", "sample"));
        }
        var report = RetroPayBundleReport.BuildRetroPayBundleReport("test", items, clock);
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
        RetroPayBundleReportCountsEachRecordOnce();
    }
}
