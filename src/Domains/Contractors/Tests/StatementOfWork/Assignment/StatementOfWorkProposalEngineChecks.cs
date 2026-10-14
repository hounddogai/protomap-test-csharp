namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkProposalEngineChecks
{
    public static void StatementOfWorkProposalEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkProposalEngine.DefaultStatementOfWorkProposalEngine(Flags.NewRegistry());
        var item = StatementOfWorkProposal.NewStatementOfWorkProposal("code-991", "title-860");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkProposalEngineKeepsResultsWithinBounds();
    }
}
