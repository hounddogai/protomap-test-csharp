namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AttestationAdjustmentReportChecks
{
    public static void AttestationAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704079624);
        var items = new List<AttestationAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AttestationAdjustment.NewAttestationAdjustment($"attestation-adjustment-{index}", "sample"));
        }
        var report = AttestationAdjustmentReport.BuildAttestationAdjustmentReport("test", items, clock);
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
        AttestationAdjustmentReportCountsEachRecordOnce();
    }
}
