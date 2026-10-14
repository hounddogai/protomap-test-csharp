namespace Acme.Workforce.Compliance.Tests.Attestation.Rollup;

using Acme.Workforce.Compliance.Attestation.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AttestationRecordReportChecks
{
    public static void AttestationRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700469316);
        var items = new List<AttestationRecord>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AttestationRecord.NewAttestationRecord($"attestation-record-{index}", "sample"));
        }
        var report = AttestationRecordReport.BuildAttestationRecordReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AttestationRecordReportCountsEachRecordOnce();
    }
}
