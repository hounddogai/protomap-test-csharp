namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WeightedAttestationEntryReportChecks
{
    public static void WeightedAttestationEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709683762);
        var items = new List<WeightedAttestationEntry>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedAttestationEntry.NewWeightedAttestationEntry($"weighted-attestation-entry-{index}", "sample"));
        }
        var report = WeightedAttestationEntryReport.BuildWeightedAttestationEntryReport("test", items, clock);
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
        WeightedAttestationEntryReportCountsEachRecordOnce();
    }
}
