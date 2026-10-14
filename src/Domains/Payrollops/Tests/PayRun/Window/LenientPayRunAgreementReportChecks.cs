namespace Acme.Workforce.Payrollops.Tests.PayRun.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LenientPayRunAgreementReportChecks
{
    public static void LenientPayRunAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702398069);
        var items = new List<LenientPayRunAgreement>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientPayRunAgreement.NewLenientPayRunAgreement($"lenient-pay-run-agreement-{index}", "sample"));
        }
        var report = LenientPayRunAgreementReport.BuildLenientPayRunAgreementReport("test", items, clock);
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
        LenientPayRunAgreementReportCountsEachRecordOnce();
    }
}
