namespace Acme.Workforce.Talent.Tests.Pool.Group;

using Acme.Workforce.Talent.Pool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PoolOverrideChecks
{
    public static void PoolOverrideStartsInNew()
    {
        var item = PoolOverride.NewPoolOverride("code-101", "name-262");
        Check.Equal(item.Status, PoolOverrideStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolOverrideMovesToRunning()
    {
        var item = PoolOverride.NewPoolOverride("code-101", "name-262");
        try
        {
            item.MoveTo(PoolOverrideStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolOverrideStatus.Running, "status");
    }

    public static void PoolOverrideRejectsMovesFromFailed()
    {
        var item = PoolOverride.NewPoolOverride("code-101", "name-262");
        item.Status = PoolOverrideStatus.Failed;
        Check.Fails(() => item.MoveTo(PoolOverrideStatus.New), "move");
    }

    public static void RunAll()
    {
        PoolOverrideStartsInNew();
        PoolOverrideMovesToRunning();
        PoolOverrideRejectsMovesFromFailed();
    }
}
