namespace Acme.Workforce.Engagement.Tests.Recognition.Allocation;

using Acme.Workforce.Engagement.Recognition.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RecognitionProposalChecks
{
    public static void RecognitionProposalStartsInPlanned()
    {
        var item = RecognitionProposal.NewRecognitionProposal("code-42", "name-778");
        Check.Equal(item.Status, RecognitionProposalStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionProposalMovesToCancelled()
    {
        var item = RecognitionProposal.NewRecognitionProposal("code-42", "name-778");
        try
        {
            item.MoveTo(RecognitionProposalStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionProposalStatus.Cancelled, "status");
    }

    public static void RecognitionProposalRejectsMovesFromCancelled()
    {
        var item = RecognitionProposal.NewRecognitionProposal("code-42", "name-778");
        item.Status = RecognitionProposalStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RecognitionProposalStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RecognitionProposalStartsInPlanned();
        RecognitionProposalMovesToCancelled();
        RecognitionProposalRejectsMovesFromCancelled();
    }
}
