namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DefaultAttestationRollupReportChecks
{
    public static void DefaultAttestationRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704523091);
        var items = new List<DefaultAttestationRollup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DefaultAttestationRollup.NewDefaultAttestationRollup($"default-attestation-rollup-{index}", "sample"));
        }
        var report = DefaultAttestationRollupReport.BuildDefaultAttestationRollupReport("test", items, clock);
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
        DefaultAttestationRollupReportCountsEachRecordOnce();
    }
}
