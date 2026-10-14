namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueExceptionChecks
{
    public static void TrainingDueExceptionStartsInCreated()
    {
        var item = TrainingDueException.NewTrainingDueException("code-327", "title-464");
        Check.Equal(item.Status, TrainingDueExceptionStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueExceptionMovesToPublished()
    {
        var item = TrainingDueException.NewTrainingDueException("code-327", "title-464");
        try
        {
            item.MoveTo(TrainingDueExceptionStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueExceptionStatus.Published, "status");
    }

    public static void TrainingDueExceptionRejectsMovesFromRetired()
    {
        var item = TrainingDueException.NewTrainingDueException("code-327", "title-464");
        item.Status = TrainingDueExceptionStatus.Retired;
        Check.Fails(() => item.MoveTo(TrainingDueExceptionStatus.Created), "move");
    }

    public static void RunAll()
    {
        TrainingDueExceptionStartsInCreated();
        TrainingDueExceptionMovesToPublished();
        TrainingDueExceptionRejectsMovesFromRetired();
    }
}
