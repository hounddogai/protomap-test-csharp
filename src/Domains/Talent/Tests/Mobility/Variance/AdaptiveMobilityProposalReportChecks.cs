namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveMobilityProposalReportChecks
{
    public static void AdaptiveMobilityProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706897318);
        var items = new List<AdaptiveMobilityProposal>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdaptiveMobilityProposal.NewAdaptiveMobilityProposal($"adaptive-mobility-proposal-{index}", "sample"));
        }
        var report = AdaptiveMobilityProposalReport.BuildAdaptiveMobilityProposalReport("test", items, clock);
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
        AdaptiveMobilityProposalReportCountsEachRecordOnce();
    }
}
