namespace Acme.Workforce.Engagement.Tests.Recognition.Allocation;

using Acme.Workforce.Engagement.Recognition.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RecognitionProposalViewChecks
{
    public static void RecognitionProposalViewShowsTheStatusInCapitals()
    {
        var item = RecognitionProposal.NewRecognitionProposal("code-42", "name-778");
        var view = RecognitionProposalView.ToRecognitionProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionProposalView.ToRecognitionProposalViews(new List<RecognitionProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionProposalViewShowsTheStatusInCapitals();
    }
}
