namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class CappedAttestationQuotaReportChecks
{
    public static void CappedAttestationQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706038368);
        var items = new List<CappedAttestationQuota>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedAttestationQuota.NewCappedAttestationQuota($"capped-attestation-quota-{index}", "sample"));
        }
        var report = CappedAttestationQuotaReport.BuildCappedAttestationQuotaReport("test", items, clock);
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
        CappedAttestationQuotaReportCountsEachRecordOnce();
    }
}
