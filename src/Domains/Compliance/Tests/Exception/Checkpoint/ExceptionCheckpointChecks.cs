namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ExceptionCheckpointChecks
{
    public static void ExceptionCheckpointStartsInCreated()
    {
        var item = ExceptionCheckpoint.NewExceptionCheckpoint("code-328", "label-568");
        Check.Equal(item.Status, ExceptionCheckpointStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionCheckpointMovesToValidated()
    {
        var item = ExceptionCheckpoint.NewExceptionCheckpoint("code-328", "label-568");
        try
        {
            item.MoveTo(ExceptionCheckpointStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionCheckpointStatus.Validated, "status");
    }

    public static void ExceptionCheckpointRejectsMovesFromRetired()
    {
        var item = ExceptionCheckpoint.NewExceptionCheckpoint("code-328", "label-568");
        item.Status = ExceptionCheckpointStatus.Retired;
        Check.Fails(() => item.MoveTo(ExceptionCheckpointStatus.Created), "move");
    }

    public static void RunAll()
    {
        ExceptionCheckpointStartsInCreated();
        ExceptionCheckpointMovesToValidated();
        ExceptionCheckpointRejectsMovesFromRetired();
    }
}
