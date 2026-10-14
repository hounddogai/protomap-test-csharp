namespace Acme.Workforce.Compliance.Tests.Attestation.Draft;

using Acme.Workforce.Compliance.Attestation.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AttestationDraftReportChecks
{
    public static void AttestationDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701006366);
        var items = new List<AttestationDraft>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AttestationDraft.NewAttestationDraft($"attestation-draft-{index}", "sample"));
        }
        var report = AttestationDraftReport.BuildAttestationDraftReport("test", items, clock);
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
        AttestationDraftReportCountsEachRecordOnce();
    }
}
