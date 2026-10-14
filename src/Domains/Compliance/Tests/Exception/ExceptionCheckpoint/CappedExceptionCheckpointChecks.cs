namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionCheckpoint;

using Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

/// <summary>Checks of exception checkpoint code, run by the project's runner.</summary>
public static class CappedExceptionCheckpointChecks
{
    public static void CappedExceptionCheckpointStartsInRequested()
    {
        var item = CappedExceptionCheckpoint.NewCappedExceptionCheckpoint("code-909", "title-108");
        Check.Equal(item.Status, CappedExceptionCheckpointStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedExceptionCheckpointMovesToExpired()
    {
        var item = CappedExceptionCheckpoint.NewCappedExceptionCheckpoint("code-909", "title-108");
        try
        {
            item.MoveTo(CappedExceptionCheckpointStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedExceptionCheckpointStatus.Expired, "status");
    }

    public static void CappedExceptionCheckpointRejectsMovesFromExpired()
    {
        var item = CappedExceptionCheckpoint.NewCappedExceptionCheckpoint("code-909", "title-108");
        item.Status = CappedExceptionCheckpointStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedExceptionCheckpointStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedExceptionCheckpointStartsInRequested();
        CappedExceptionCheckpointMovesToExpired();
        CappedExceptionCheckpointRejectsMovesFromExpired();
    }
}
