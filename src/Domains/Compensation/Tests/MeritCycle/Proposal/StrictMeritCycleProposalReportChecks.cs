namespace Acme.Workforce.Compensation.Tests.MeritCycle.Proposal;

using Acme.Workforce.Compensation.MeritCycle.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class StrictMeritCycleProposalReportChecks
{
    public static void StrictMeritCycleProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706166943);
        var items = new List<StrictMeritCycleProposal>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StrictMeritCycleProposal.NewStrictMeritCycleProposal($"strict-merit-cycle-proposal-{index}", "sample"));
        }
        var report = StrictMeritCycleProposalReport.BuildStrictMeritCycleProposalReport("test", items, clock);
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
        StrictMeritCycleProposalReportCountsEachRecordOnce();
    }
}
