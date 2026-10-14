namespace Acme.Workforce.Payrollops.Tests.PayRun.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PayRunAgreementReportChecks
{
    public static void PayRunAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701752437);
        var items = new List<PayRunAgreement>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PayRunAgreement.NewPayRunAgreement($"pay-run-agreement-{index}", "sample"));
        }
        var report = PayRunAgreementReport.BuildPayRunAgreementReport("test", items, clock);
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
        PayRunAgreementReportCountsEachRecordOnce();
    }
}
