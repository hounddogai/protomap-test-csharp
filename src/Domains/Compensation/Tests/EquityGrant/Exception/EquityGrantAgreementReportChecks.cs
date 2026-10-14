namespace Acme.Workforce.Compensation.Tests.EquityGrant.Exception;

using Acme.Workforce.Compensation.EquityGrant.Exception;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class EquityGrantAgreementReportChecks
{
    public static void EquityGrantAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709178128);
        var items = new List<EquityGrantAgreement>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(EquityGrantAgreement.NewEquityGrantAgreement($"equity-grant-agreement-{index}", "sample"));
        }
        var report = EquityGrantAgreementReport.BuildEquityGrantAgreementReport("test", items, clock);
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
        EquityGrantAgreementReportCountsEachRecordOnce();
    }
}
