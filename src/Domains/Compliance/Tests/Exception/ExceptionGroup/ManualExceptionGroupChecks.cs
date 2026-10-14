namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionGroup;

using Acme.Workforce.Compliance.Exception.ExceptionGroup;

/// <summary>Checks of exception group code, run by the project's runner.</summary>
public static class ManualExceptionGroupChecks
{
    public static void ManualExceptionGroupStartsInIdle()
    {
        var item = ManualExceptionGroup.NewManualExceptionGroup("code-662", "name-338");
        Check.Equal(item.Status, ManualExceptionGroupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualExceptionGroupMovesToDraining()
    {
        var item = ManualExceptionGroup.NewManualExceptionGroup("code-662", "name-338");
        try
        {
            item.MoveTo(ManualExceptionGroupStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualExceptionGroupStatus.Draining, "status");
    }

    public static void ManualExceptionGroupRejectsMovesFromStopped()
    {
        var item = ManualExceptionGroup.NewManualExceptionGroup("code-662", "name-338");
        item.Status = ManualExceptionGroupStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualExceptionGroupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualExceptionGroupStartsInIdle();
        ManualExceptionGroupMovesToDraining();
        ManualExceptionGroupRejectsMovesFromStopped();
    }
}
