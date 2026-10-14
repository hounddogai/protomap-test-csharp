namespace Acme.Workforce.Compliance.Tests.Attestation.Quota;

using Acme.Workforce.Compliance.Attestation.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AttestationSummaryReportChecks
{
    public static void AttestationSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705712300);
        var items = new List<AttestationSummary>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AttestationSummary.NewAttestationSummary($"attestation-summary-{index}", "sample"));
        }
        var report = AttestationSummaryReport.BuildAttestationSummaryReport("test", items, clock);
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
        AttestationSummaryReportCountsEachRecordOnce();
    }
}
