namespace Acme.Workforce.Assets.Tests.Assignment.Proposal;

using Acme.Workforce.Assets.Assignment.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SeasonalAssignmentProposalReportChecks
{
    public static void SeasonalAssignmentProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706587892);
        var items = new List<SeasonalAssignmentProposal>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SeasonalAssignmentProposal.NewSeasonalAssignmentProposal($"seasonal-assignment-proposal-{index}", "sample"));
        }
        var report = SeasonalAssignmentProposalReport.BuildSeasonalAssignmentProposalReport("test", items, clock);
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
        SeasonalAssignmentProposalReportCountsEachRecordOnce();
    }
}
