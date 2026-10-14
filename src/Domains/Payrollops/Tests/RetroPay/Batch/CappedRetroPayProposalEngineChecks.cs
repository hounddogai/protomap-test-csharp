namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class CappedRetroPayProposalEngineChecks
{
    public static void CappedRetroPayProposalEngineKeepsResultsWithinBounds()
    {
        var engine = CappedRetroPayProposalEngine.DefaultCappedRetroPayProposalEngine(Flags.NewRegistry());
        var item = CappedRetroPayProposal.NewCappedRetroPayProposal("code-560", "title-351");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedRetroPayProposalEngineKeepsResultsWithinBounds();
    }
}
