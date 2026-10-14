namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PoolProfileChecks
{
    public static void PoolProfileStartsInDraft()
    {
        var item = PoolProfile.NewPoolProfile("code-14", "label-680");
        Check.Equal(item.Status, PoolProfileStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolProfileMovesToPending()
    {
        var item = PoolProfile.NewPoolProfile("code-14", "label-680");
        try
        {
            item.MoveTo(PoolProfileStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolProfileStatus.Pending, "status");
    }

    public static void PoolProfileRejectsMovesFromClosed()
    {
        var item = PoolProfile.NewPoolProfile("code-14", "label-680");
        item.Status = PoolProfileStatus.Closed;
        Check.Fails(() => item.MoveTo(PoolProfileStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PoolProfileStartsInDraft();
        PoolProfileMovesToPending();
        PoolProfileRejectsMovesFromClosed();
    }
}
