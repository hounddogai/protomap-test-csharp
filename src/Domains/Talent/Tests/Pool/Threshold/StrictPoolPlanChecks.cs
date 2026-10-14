namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StrictPoolPlanChecks
{
    public static void StrictPoolPlanStartsInRequested()
    {
        var item = StrictPoolPlan.NewStrictPoolPlan("code-451", "label-857");
        Check.Equal(item.Status, StrictPoolPlanStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPoolPlanMovesToRevoked()
    {
        var item = StrictPoolPlan.NewStrictPoolPlan("code-451", "label-857");
        try
        {
            item.MoveTo(StrictPoolPlanStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPoolPlanStatus.Revoked, "status");
    }

    public static void StrictPoolPlanRejectsMovesFromExpired()
    {
        var item = StrictPoolPlan.NewStrictPoolPlan("code-451", "label-857");
        item.Status = StrictPoolPlanStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictPoolPlanStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictPoolPlanStartsInRequested();
        StrictPoolPlanMovesToRevoked();
        StrictPoolPlanRejectsMovesFromExpired();
    }
}
