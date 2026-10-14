namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateProposalEngineChecks
{
    public static void RateProposalEngineKeepsResultsWithinBounds()
    {
        var engine = RateProposalEngine.DefaultRateProposalEngine();
        var item = RateProposal.NewRateProposal("code-613", "title-743");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateProposalEngineKeepsResultsWithinBounds();
    }
}
