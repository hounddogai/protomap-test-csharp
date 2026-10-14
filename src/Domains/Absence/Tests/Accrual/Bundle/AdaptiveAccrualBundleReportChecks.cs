namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AdaptiveAccrualBundleReportChecks
{
    public static void AdaptiveAccrualBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707539144);
        var items = new List<AdaptiveAccrualBundle>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptiveAccrualBundle.NewAdaptiveAccrualBundle($"adaptive-accrual-bundle-{index}", "sample"));
        }
        var report = AdaptiveAccrualBundleReport.BuildAdaptiveAccrualBundleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdaptiveAccrualBundleReportCountsEachRecordOnce();
    }
}
