namespace Acme.Workforce.Compensation.Tests.Benchmark.Preference;

using Acme.Workforce.Compensation.Benchmark.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RollingBenchmarkProposalChecks
{
    public static void RollingBenchmarkProposalStartsInDraft()
    {
        var item = RollingBenchmarkProposal.NewRollingBenchmarkProposal("code-387", "name-560");
        Check.Equal(item.Status, RollingBenchmarkProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingBenchmarkProposalMovesToPending()
    {
        var item = RollingBenchmarkProposal.NewRollingBenchmarkProposal("code-387", "name-560");
        try
        {
            item.MoveTo(RollingBenchmarkProposalStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingBenchmarkProposalStatus.Pending, "status");
    }

    public static void RollingBenchmarkProposalRejectsMovesFromClosed()
    {
        var item = RollingBenchmarkProposal.NewRollingBenchmarkProposal("code-387", "name-560");
        item.Status = RollingBenchmarkProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(RollingBenchmarkProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RollingBenchmarkProposalStartsInDraft();
        RollingBenchmarkProposalMovesToPending();
        RollingBenchmarkProposalRejectsMovesFromClosed();
    }
}
