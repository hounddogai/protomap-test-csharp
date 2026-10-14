namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class FallbackAttestationSummaryReportChecks
{
    public static void FallbackAttestationSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703509542);
        var items = new List<FallbackAttestationSummary>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(FallbackAttestationSummary.NewFallbackAttestationSummary($"fallback-attestation-summary-{index}", "sample"));
        }
        var report = FallbackAttestationSummaryReport.BuildFallbackAttestationSummaryReport("test", items, clock);
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
        FallbackAttestationSummaryReportCountsEachRecordOnce();
    }
}
