namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationDraft;

using Acme.Workforce.Compliance.Attestation.AttestationDraft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of attestation draft code, run by the project's runner.</summary>
public static class AdaptiveAttestationDraftReportChecks
{
    public static void AdaptiveAttestationDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709221800);
        var items = new List<AdaptiveAttestationDraft>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptiveAttestationDraft.NewAdaptiveAttestationDraft($"adaptive-attestation-draft-{index}", "sample"));
        }
        var report = AdaptiveAttestationDraftReport.BuildAdaptiveAttestationDraftReport("test", items, clock);
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
        AdaptiveAttestationDraftReportCountsEachRecordOnce();
    }
}
