namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictSuccessionPlanOverrideChecks
{
    public static void StrictSuccessionPlanOverrideStartsInRequested()
    {
        var item = StrictSuccessionPlanOverride.NewStrictSuccessionPlanOverride("code-256", "title-868");
        Check.Equal(item.Status, StrictSuccessionPlanOverrideStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictSuccessionPlanOverrideMovesToGranted()
    {
        var item = StrictSuccessionPlanOverride.NewStrictSuccessionPlanOverride("code-256", "title-868");
        try
        {
            item.MoveTo(StrictSuccessionPlanOverrideStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictSuccessionPlanOverrideStatus.Granted, "status");
    }

    public static void StrictSuccessionPlanOverrideRejectsMovesFromExpired()
    {
        var item = StrictSuccessionPlanOverride.NewStrictSuccessionPlanOverride("code-256", "title-868");
        item.Status = StrictSuccessionPlanOverrideStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictSuccessionPlanOverrideStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictSuccessionPlanOverrideStartsInRequested();
        StrictSuccessionPlanOverrideMovesToGranted();
        StrictSuccessionPlanOverrideRejectsMovesFromExpired();
    }
}
