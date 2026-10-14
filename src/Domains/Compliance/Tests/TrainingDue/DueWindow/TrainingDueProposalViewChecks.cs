namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class TrainingDueProposalViewChecks
{
    public static void TrainingDueProposalViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueProposal.NewTrainingDueProposal("code-483", "name-253");
        var view = TrainingDueProposalView.ToTrainingDueProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueProposalView.ToTrainingDueProposalViews(new List<TrainingDueProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueProposalViewShowsTheStatusInCapitals();
    }
}
