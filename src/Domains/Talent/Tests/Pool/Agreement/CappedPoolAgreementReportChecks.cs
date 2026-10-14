namespace Acme.Workforce.Talent.Tests.Pool.Agreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CappedPoolAgreementReportChecks
{
    public static void CappedPoolAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702344421);
        var items = new List<CappedPoolAgreement>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CappedPoolAgreement.NewCappedPoolAgreement($"capped-pool-agreement-{index}", "sample"));
        }
        var report = CappedPoolAgreementReport.BuildCappedPoolAgreementReport("test", items, clock);
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
        CappedPoolAgreementReportCountsEachRecordOnce();
    }
}
