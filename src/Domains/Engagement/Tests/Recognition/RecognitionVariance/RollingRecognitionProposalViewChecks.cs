namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class RollingRecognitionProposalViewChecks
{
    public static void RollingRecognitionProposalViewShowsTheStatusInCapitals()
    {
        var item = RollingRecognitionProposal.NewRollingRecognitionProposal("code-295", "label-71");
        var view = RollingRecognitionProposalView.ToRollingRecognitionProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingRecognitionProposalView.ToRollingRecognitionProposalViews(new List<RollingRecognitionProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingRecognitionProposalViewShowsTheStatusInCapitals();
    }
}
