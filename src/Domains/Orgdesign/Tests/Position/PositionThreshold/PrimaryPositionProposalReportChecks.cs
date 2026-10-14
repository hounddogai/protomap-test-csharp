namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class PrimaryPositionProposalReportChecks
{
    public static void PrimaryPositionProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701774316);
        var items = new List<PrimaryPositionProposal>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PrimaryPositionProposal.NewPrimaryPositionProposal($"primary-position-proposal-{index}", "sample"));
        }
        var report = PrimaryPositionProposalReport.BuildPrimaryPositionProposalReport("test", items, clock);
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
        PrimaryPositionProposalReportCountsEachRecordOnce();
    }
}
