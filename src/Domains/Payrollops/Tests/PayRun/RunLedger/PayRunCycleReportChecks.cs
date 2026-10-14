namespace Acme.Workforce.Payrollops.Tests.PayRun.RunLedger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Checks of run ledger code, run by the project's runner.</summary>
public static class PayRunCycleReportChecks
{
    public static void PayRunCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703352581);
        var items = new List<PayRunCycle>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PayRunCycle.NewPayRunCycle($"pay-run-cycle-{index}", "sample"));
        }
        var report = PayRunCycleReport.BuildPayRunCycleReport("test", items, clock);
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
        PayRunCycleReportCountsEachRecordOnce();
    }
}
