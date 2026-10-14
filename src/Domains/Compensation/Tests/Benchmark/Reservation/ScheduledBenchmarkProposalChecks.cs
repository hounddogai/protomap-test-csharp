namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ScheduledBenchmarkProposalChecks
{
    public static void ScheduledBenchmarkProposalStartsInDraft()
    {
        var item = ScheduledBenchmarkProposal.NewScheduledBenchmarkProposal("code-743", "title-987");
        Check.Equal(item.Status, ScheduledBenchmarkProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledBenchmarkProposalMovesToPending()
    {
        var item = ScheduledBenchmarkProposal.NewScheduledBenchmarkProposal("code-743", "title-987");
        try
        {
            item.MoveTo(ScheduledBenchmarkProposalStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledBenchmarkProposalStatus.Pending, "status");
    }

    public static void ScheduledBenchmarkProposalRejectsMovesFromClosed()
    {
        var item = ScheduledBenchmarkProposal.NewScheduledBenchmarkProposal("code-743", "title-987");
        item.Status = ScheduledBenchmarkProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledBenchmarkProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledBenchmarkProposalStartsInDraft();
        ScheduledBenchmarkProposalMovesToPending();
        ScheduledBenchmarkProposalRejectsMovesFromClosed();
    }
}
