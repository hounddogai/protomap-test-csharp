namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class DefaultReorgExceptionChecks
{
    public static void DefaultReorgExceptionStartsInIdle()
    {
        var item = DefaultReorgException.NewDefaultReorgException("code-151", "title-650");
        Check.Equal(item.Status, DefaultReorgExceptionStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultReorgExceptionMovesToReady()
    {
        var item = DefaultReorgException.NewDefaultReorgException("code-151", "title-650");
        try
        {
            item.MoveTo(DefaultReorgExceptionStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultReorgExceptionStatus.Ready, "status");
    }

    public static void DefaultReorgExceptionRejectsMovesFromStopped()
    {
        var item = DefaultReorgException.NewDefaultReorgException("code-151", "title-650");
        item.Status = DefaultReorgExceptionStatus.Stopped;
        Check.Fails(() => item.MoveTo(DefaultReorgExceptionStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DefaultReorgExceptionStartsInIdle();
        DefaultReorgExceptionMovesToReady();
        DefaultReorgExceptionRejectsMovesFromStopped();
    }
}
