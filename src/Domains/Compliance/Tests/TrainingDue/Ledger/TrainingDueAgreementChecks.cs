namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TrainingDueAgreementChecks
{
    public static void TrainingDueAgreementStartsInOpen()
    {
        var item = TrainingDueAgreement.NewTrainingDueAgreement("code-997", "label-182");
        Check.Equal(item.Status, TrainingDueAgreementStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueAgreementMovesToReopened()
    {
        var item = TrainingDueAgreement.NewTrainingDueAgreement("code-997", "label-182");
        try
        {
            item.MoveTo(TrainingDueAgreementStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueAgreementStatus.Reopened, "status");
    }

    public static void TrainingDueAgreementRejectsMovesFromArchived()
    {
        var item = TrainingDueAgreement.NewTrainingDueAgreement("code-997", "label-182");
        item.Status = TrainingDueAgreementStatus.Archived;
        Check.Fails(() => item.MoveTo(TrainingDueAgreementStatus.Open), "move");
    }

    public static void RunAll()
    {
        TrainingDueAgreementStartsInOpen();
        TrainingDueAgreementMovesToReopened();
        TrainingDueAgreementRejectsMovesFromArchived();
    }
}
