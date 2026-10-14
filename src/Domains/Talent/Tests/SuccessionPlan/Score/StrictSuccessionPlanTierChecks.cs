namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictSuccessionPlanTierChecks
{
    public static void StrictSuccessionPlanTierStartsInDraft()
    {
        var item = StrictSuccessionPlanTier.NewStrictSuccessionPlanTier("code-235", "name-157");
        Check.Equal(item.Status, StrictSuccessionPlanTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictSuccessionPlanTierMovesToPending()
    {
        var item = StrictSuccessionPlanTier.NewStrictSuccessionPlanTier("code-235", "name-157");
        try
        {
            item.MoveTo(StrictSuccessionPlanTierStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictSuccessionPlanTierStatus.Pending, "status");
    }

    public static void StrictSuccessionPlanTierRejectsMovesFromClosed()
    {
        var item = StrictSuccessionPlanTier.NewStrictSuccessionPlanTier("code-235", "name-157");
        item.Status = StrictSuccessionPlanTierStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictSuccessionPlanTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictSuccessionPlanTierStartsInDraft();
        StrictSuccessionPlanTierMovesToPending();
        StrictSuccessionPlanTierRejectsMovesFromClosed();
    }
}
