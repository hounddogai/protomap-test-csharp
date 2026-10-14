namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class TrainingDueProposalChecks
{
    public static void TrainingDueProposalStartsInDraft()
    {
        var item = TrainingDueProposal.NewTrainingDueProposal("code-483", "name-253");
        Check.Equal(item.Status, TrainingDueProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueProposalMovesToActive()
    {
        var item = TrainingDueProposal.NewTrainingDueProposal("code-483", "name-253");
        try
        {
            item.MoveTo(TrainingDueProposalStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueProposalStatus.Active, "status");
    }

    public static void TrainingDueProposalRejectsMovesFromClosed()
    {
        var item = TrainingDueProposal.NewTrainingDueProposal("code-483", "name-253");
        item.Status = TrainingDueProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(TrainingDueProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TrainingDueProposalStartsInDraft();
        TrainingDueProposalMovesToActive();
        TrainingDueProposalRejectsMovesFromClosed();
    }
}
