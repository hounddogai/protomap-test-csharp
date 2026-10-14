namespace Acme.Workforce.Compliance.Tests.Attestation.Quota;

using Acme.Workforce.Compliance.Attestation.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AttestationMappingReportChecks
{
    public static void AttestationMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700359262);
        var items = new List<AttestationMapping>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AttestationMapping.NewAttestationMapping($"attestation-mapping-{index}", "sample"));
        }
        var report = AttestationMappingReport.BuildAttestationMappingReport("test", items, clock);
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
        AttestationMappingReportCountsEachRecordOnce();
    }
}
