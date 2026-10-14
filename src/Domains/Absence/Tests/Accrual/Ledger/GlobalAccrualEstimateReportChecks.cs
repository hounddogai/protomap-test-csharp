namespace Acme.Workforce.Absence.Tests.Accrual.Ledger;

using Acme.Workforce.Absence.Accrual.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class GlobalAccrualEstimateReportChecks
{
    public static void GlobalAccrualEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709993900);
        var items = new List<GlobalAccrualEstimate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(GlobalAccrualEstimate.NewGlobalAccrualEstimate($"global-accrual-estimate-{index}", "sample"));
        }
        var report = GlobalAccrualEstimateReport.BuildGlobalAccrualEstimateReport("test", items, clock);
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
        GlobalAccrualEstimateReportCountsEachRecordOnce();
    }
}
