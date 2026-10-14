namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentProposalChecks
{
    public static void SentimentProposalStartsInPlanned()
    {
        var item = SentimentProposal.NewSentimentProposal("code-555", "label-184");
        Check.Equal(item.Status, SentimentProposalStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentProposalMovesToScheduled()
    {
        var item = SentimentProposal.NewSentimentProposal("code-555", "label-184");
        try
        {
            item.MoveTo(SentimentProposalStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentProposalStatus.Scheduled, "status");
    }

    public static void SentimentProposalRejectsMovesFromCancelled()
    {
        var item = SentimentProposal.NewSentimentProposal("code-555", "label-184");
        item.Status = SentimentProposalStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SentimentProposalStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SentimentProposalStartsInPlanned();
        SentimentProposalMovesToScheduled();
        SentimentProposalRejectsMovesFromCancelled();
    }
}
