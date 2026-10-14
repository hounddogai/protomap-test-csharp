namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RetroPayAgreementReportChecks
{
    public static void RetroPayAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700416936);
        var items = new List<RetroPayAgreement>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RetroPayAgreement.NewRetroPayAgreement($"retro-pay-agreement-{index}", "sample"));
        }
        var report = RetroPayAgreementReport.BuildRetroPayAgreementReport("test", items, clock);
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
        RetroPayAgreementReportCountsEachRecordOnce();
    }
}
