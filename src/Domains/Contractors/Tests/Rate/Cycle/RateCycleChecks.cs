namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RateCycleChecks
{
    public static void RateCycleStartsInProposed()
    {
        var item = RateCycle.NewRateCycle("code-881", "title-15");
        Check.Equal(item.Status, RateCycleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateCycleMovesToWithdrawn()
    {
        var item = RateCycle.NewRateCycle("code-881", "title-15");
        try
        {
            item.MoveTo(RateCycleStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateCycleStatus.Withdrawn, "status");
    }

    public static void RateCycleRejectsMovesFromWithdrawn()
    {
        var item = RateCycle.NewRateCycle("code-881", "title-15");
        item.Status = RateCycleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RateCycleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RateCycleStartsInProposed();
        RateCycleMovesToWithdrawn();
        RateCycleRejectsMovesFromWithdrawn();
    }
}
