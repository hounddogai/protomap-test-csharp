namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueTierChecks
{
    public static void TrainingDueTierStartsInCreated()
    {
        var item = TrainingDueTier.NewTrainingDueTier("code-697", "label-128");
        Check.Equal(item.Status, TrainingDueTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueTierMovesToPublished()
    {
        var item = TrainingDueTier.NewTrainingDueTier("code-697", "label-128");
        try
        {
            item.MoveTo(TrainingDueTierStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueTierStatus.Published, "status");
    }

    public static void TrainingDueTierRejectsMovesFromRetired()
    {
        var item = TrainingDueTier.NewTrainingDueTier("code-697", "label-128");
        item.Status = TrainingDueTierStatus.Retired;
        Check.Fails(() => item.MoveTo(TrainingDueTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        TrainingDueTierStartsInCreated();
        TrainingDueTierMovesToPublished();
        TrainingDueTierRejectsMovesFromRetired();
    }
}
