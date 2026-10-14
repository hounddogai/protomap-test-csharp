namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RateOverrideChecks
{
    public static void RateOverrideStartsInProposed()
    {
        var item = RateOverride.NewRateOverride("code-381", "name-424");
        Check.Equal(item.Status, RateOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateOverrideMovesToApproved()
    {
        var item = RateOverride.NewRateOverride("code-381", "name-424");
        try
        {
            item.MoveTo(RateOverrideStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateOverrideStatus.Approved, "status");
    }

    public static void RateOverrideRejectsMovesFromWithdrawn()
    {
        var item = RateOverride.NewRateOverride("code-381", "name-424");
        item.Status = RateOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RateOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RateOverrideStartsInProposed();
        RateOverrideMovesToApproved();
        RateOverrideRejectsMovesFromWithdrawn();
    }
}
