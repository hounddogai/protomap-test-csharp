namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PoolWindowChecks
{
    public static void PoolWindowStartsInPlanned()
    {
        var item = PoolWindow.NewPoolWindow("code-431", "title-338");
        Check.Equal(item.Status, PoolWindowStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolWindowMovesToCompleted()
    {
        var item = PoolWindow.NewPoolWindow("code-431", "title-338");
        try
        {
            item.MoveTo(PoolWindowStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolWindowStatus.Completed, "status");
    }

    public static void PoolWindowRejectsMovesFromCancelled()
    {
        var item = PoolWindow.NewPoolWindow("code-431", "title-338");
        item.Status = PoolWindowStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PoolWindowStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PoolWindowStartsInPlanned();
        PoolWindowMovesToCompleted();
        PoolWindowRejectsMovesFromCancelled();
    }
}
