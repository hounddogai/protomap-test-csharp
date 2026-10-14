namespace Acme.Workforce.Compliance.Tests.Attestation.Segment;

using Acme.Workforce.Compliance.Attestation.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CappedAttestationProposalReportChecks
{
    public static void CappedAttestationProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705841057);
        var items = new List<CappedAttestationProposal>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedAttestationProposal.NewCappedAttestationProposal($"capped-attestation-proposal-{index}", "sample"));
        }
        var report = CappedAttestationProposalReport.BuildCappedAttestationProposalReport("test", items, clock);
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
        CappedAttestationProposalReportCountsEachRecordOnce();
    }
}
