namespace Acme.Workforce.Orgdesign.Tests.Position.Audit;

using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class GlobalPositionExceptionChecks
{
    public static void GlobalPositionExceptionStartsInRequested()
    {
        var item = GlobalPositionException.NewGlobalPositionException("code-890", "name-519");
        Check.Equal(item.Status, GlobalPositionExceptionStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPositionExceptionMovesToGranted()
    {
        var item = GlobalPositionException.NewGlobalPositionException("code-890", "name-519");
        try
        {
            item.MoveTo(GlobalPositionExceptionStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPositionExceptionStatus.Granted, "status");
    }

    public static void GlobalPositionExceptionRejectsMovesFromExpired()
    {
        var item = GlobalPositionException.NewGlobalPositionException("code-890", "name-519");
        item.Status = GlobalPositionExceptionStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalPositionExceptionStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalPositionExceptionStartsInRequested();
        GlobalPositionExceptionMovesToGranted();
        GlobalPositionExceptionRejectsMovesFromExpired();
    }
}
