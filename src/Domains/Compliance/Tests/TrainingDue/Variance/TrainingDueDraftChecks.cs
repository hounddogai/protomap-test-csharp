namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueDraftChecks
{
    public static void TrainingDueDraftStartsInNew()
    {
        var item = TrainingDueDraft.NewTrainingDueDraft("code-518", "title-545");
        Check.Equal(item.Status, TrainingDueDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueDraftMovesToRunning()
    {
        var item = TrainingDueDraft.NewTrainingDueDraft("code-518", "title-545");
        try
        {
            item.MoveTo(TrainingDueDraftStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueDraftStatus.Running, "status");
    }

    public static void TrainingDueDraftRejectsMovesFromFailed()
    {
        var item = TrainingDueDraft.NewTrainingDueDraft("code-518", "title-545");
        item.Status = TrainingDueDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(TrainingDueDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        TrainingDueDraftStartsInNew();
        TrainingDueDraftMovesToRunning();
        TrainingDueDraftRejectsMovesFromFailed();
    }
}
