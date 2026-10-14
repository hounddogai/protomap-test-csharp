namespace Acme.Workforce.Payrollops.Tests.PayRun.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PayRunMappingReportChecks
{
    public static void PayRunMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702848898);
        var items = new List<PayRunMapping>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PayRunMapping.NewPayRunMapping($"pay-run-mapping-{index}", "sample"));
        }
        var report = PayRunMappingReport.BuildPayRunMappingReport("test", items, clock);
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
        PayRunMappingReportCountsEachRecordOnce();
    }
}
