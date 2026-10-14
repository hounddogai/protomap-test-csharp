namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class GlobalPayBandAgreementReportChecks
{
    public static void GlobalPayBandAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708756202);
        var items = new List<GlobalPayBandAgreement>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(GlobalPayBandAgreement.NewGlobalPayBandAgreement($"global-pay-band-agreement-{index}", "sample"));
        }
        var report = GlobalPayBandAgreementReport.BuildGlobalPayBandAgreementReport("test", items, clock);
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
        GlobalPayBandAgreementReportCountsEachRecordOnce();
    }
}
