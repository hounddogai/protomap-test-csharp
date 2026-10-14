namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class ManualAttestationEstimateReportChecks
{
    public static void ManualAttestationEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705898905);
        var items = new List<ManualAttestationEstimate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ManualAttestationEstimate.NewManualAttestationEstimate($"manual-attestation-estimate-{index}", "sample"));
        }
        var report = ManualAttestationEstimateReport.BuildManualAttestationEstimateReport("test", items, clock);
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
        ManualAttestationEstimateReportCountsEachRecordOnce();
    }
}
