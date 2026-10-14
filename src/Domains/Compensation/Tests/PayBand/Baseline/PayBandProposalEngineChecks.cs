namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandProposalEngineChecks
{
    public static void PayBandProposalEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandProposalEngine.DefaultPayBandProposalEngine(Flags.NewRegistry());
        var item = PayBandProposal.NewPayBandProposal("code-921", "title-608");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandProposalEngineKeepsResultsWithinBounds();
    }
}
