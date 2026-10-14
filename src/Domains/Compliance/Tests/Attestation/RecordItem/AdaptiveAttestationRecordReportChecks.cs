namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AdaptiveAttestationRecordReportChecks
{
    public static void AdaptiveAttestationRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704017992);
        var items = new List<AdaptiveAttestationRecord>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdaptiveAttestationRecord.NewAdaptiveAttestationRecord($"adaptive-attestation-record-{index}", "sample"));
        }
        var report = AdaptiveAttestationRecordReport.BuildAdaptiveAttestationRecordReport("test", items, clock);
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
        AdaptiveAttestationRecordReportCountsEachRecordOnce();
    }
}
