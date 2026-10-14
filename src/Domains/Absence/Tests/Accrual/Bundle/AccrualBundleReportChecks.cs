namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AccrualBundleReportChecks
{
    public static void AccrualBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702415914);
        var items = new List<AccrualBundle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AccrualBundle.NewAccrualBundle($"accrual-bundle-{index}", "sample"));
        }
        var report = AccrualBundleReport.BuildAccrualBundleReport("test", items, clock);
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
        AccrualBundleReportCountsEachRecordOnce();
    }
}
