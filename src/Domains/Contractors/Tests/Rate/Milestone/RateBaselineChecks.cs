namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class RateBaselineChecks
{
    public static void RateBaselineStartsInIdle()
    {
        var item = RateBaseline.NewRateBaseline("code-410", "name-822");
        Check.Equal(item.Status, RateBaselineStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateBaselineMovesToDraining()
    {
        var item = RateBaseline.NewRateBaseline("code-410", "name-822");
        try
        {
            item.MoveTo(RateBaselineStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateBaselineStatus.Draining, "status");
    }

    public static void RateBaselineRejectsMovesFromStopped()
    {
        var item = RateBaseline.NewRateBaseline("code-410", "name-822");
        item.Status = RateBaselineStatus.Stopped;
        Check.Fails(() => item.MoveTo(RateBaselineStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RateBaselineStartsInIdle();
        RateBaselineMovesToDraining();
        RateBaselineRejectsMovesFromStopped();
    }
}
