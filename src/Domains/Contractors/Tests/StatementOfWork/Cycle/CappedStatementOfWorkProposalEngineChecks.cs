namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class CappedStatementOfWorkProposalEngineChecks
{
    public static void CappedStatementOfWorkProposalEngineKeepsResultsWithinBounds()
    {
        var engine = CappedStatementOfWorkProposalEngine.DefaultCappedStatementOfWorkProposalEngine(Flags.NewRegistry());
        var item = CappedStatementOfWorkProposal.NewCappedStatementOfWorkProposal("code-530", "name-991");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkProposalEngineKeepsResultsWithinBounds();
    }
}
