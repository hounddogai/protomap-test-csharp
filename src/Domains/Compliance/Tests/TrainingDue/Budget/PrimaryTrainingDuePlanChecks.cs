namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PrimaryTrainingDuePlanChecks
{
    public static void PrimaryTrainingDuePlanStartsInOpen()
    {
        var item = PrimaryTrainingDuePlan.NewPrimaryTrainingDuePlan("code-861", "title-972");
        Check.Equal(item.Status, PrimaryTrainingDuePlanStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryTrainingDuePlanMovesToInReview()
    {
        var item = PrimaryTrainingDuePlan.NewPrimaryTrainingDuePlan("code-861", "title-972");
        try
        {
            item.MoveTo(PrimaryTrainingDuePlanStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryTrainingDuePlanStatus.InReview, "status");
    }

    public static void PrimaryTrainingDuePlanRejectsMovesFromArchived()
    {
        var item = PrimaryTrainingDuePlan.NewPrimaryTrainingDuePlan("code-861", "title-972");
        item.Status = PrimaryTrainingDuePlanStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryTrainingDuePlanStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryTrainingDuePlanStartsInOpen();
        PrimaryTrainingDuePlanMovesToInReview();
        PrimaryTrainingDuePlanRejectsMovesFromArchived();
    }
}
