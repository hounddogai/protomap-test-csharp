namespace Acme.Workforce.Orgdesign.Tests.Headcount.Proposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HeadcountProposalReportChecks
{
    public static void HeadcountProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700759998);
        var items = new List<HeadcountProposal>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(HeadcountProposal.NewHeadcountProposal($"headcount-proposal-{index}", "sample"));
        }
        var report = HeadcountProposalReport.BuildHeadcountProposalReport("test", items, clock);
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
        HeadcountProposalReportCountsEachRecordOnce();
    }
}
