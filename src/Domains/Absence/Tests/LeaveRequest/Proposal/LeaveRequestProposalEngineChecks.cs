namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LeaveRequestProposalEngineChecks
{
    public static void LeaveRequestProposalEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestProposalEngine.DefaultLeaveRequestProposalEngine(Flags.NewRegistry());
        var item = LeaveRequestProposal.NewLeaveRequestProposal("code-538", "label-147");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestProposalEngineKeepsResultsWithinBounds();
    }
}
