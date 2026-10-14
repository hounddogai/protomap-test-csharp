namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DefaultRateCycleChecks
{
    public static void DefaultRateCycleStartsInProposed()
    {
        var item = DefaultRateCycle.NewDefaultRateCycle("code-932", "title-765");
        Check.Equal(item.Status, DefaultRateCycleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultRateCycleMovesToWithdrawn()
    {
        var item = DefaultRateCycle.NewDefaultRateCycle("code-932", "title-765");
        try
        {
            item.MoveTo(DefaultRateCycleStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultRateCycleStatus.Withdrawn, "status");
    }

    public static void DefaultRateCycleRejectsMovesFromWithdrawn()
    {
        var item = DefaultRateCycle.NewDefaultRateCycle("code-932", "title-765");
        item.Status = DefaultRateCycleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultRateCycleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultRateCycleStartsInProposed();
        DefaultRateCycleMovesToWithdrawn();
        DefaultRateCycleRejectsMovesFromWithdrawn();
    }
}
