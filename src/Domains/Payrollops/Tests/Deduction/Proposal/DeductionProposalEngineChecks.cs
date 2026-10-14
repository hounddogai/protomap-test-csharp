namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeductionProposalEngineChecks
{
    public static void DeductionProposalEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionProposalEngine.DefaultDeductionProposalEngine(Flags.NewRegistry());
        var item = DeductionProposal.NewDeductionProposal("code-842", "name-322");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionProposalEngineKeepsResultsWithinBounds();
    }
}
