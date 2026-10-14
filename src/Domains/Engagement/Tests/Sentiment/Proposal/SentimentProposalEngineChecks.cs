namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentProposalEngineChecks
{
    public static void SentimentProposalEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentProposalEngine.DefaultSentimentProposalEngine(Flags.NewRegistry());
        var item = SentimentProposal.NewSentimentProposal("code-555", "label-184");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentProposalEngineKeepsResultsWithinBounds();
    }
}
