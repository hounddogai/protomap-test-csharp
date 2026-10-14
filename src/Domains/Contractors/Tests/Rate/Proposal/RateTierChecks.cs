namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RateTierChecks
{
    public static void RateTierStartsInPlanned()
    {
        var item = RateTier.NewRateTier("code-101", "name-832");
        Check.Equal(item.Status, RateTierStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateTierMovesToScheduled()
    {
        var item = RateTier.NewRateTier("code-101", "name-832");
        try
        {
            item.MoveTo(RateTierStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateTierStatus.Scheduled, "status");
    }

    public static void RateTierRejectsMovesFromCancelled()
    {
        var item = RateTier.NewRateTier("code-101", "name-832");
        item.Status = RateTierStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RateTierStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RateTierStartsInPlanned();
        RateTierMovesToScheduled();
        RateTierRejectsMovesFromCancelled();
    }
}
