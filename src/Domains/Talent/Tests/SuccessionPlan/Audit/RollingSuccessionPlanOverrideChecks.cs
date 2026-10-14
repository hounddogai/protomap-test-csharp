namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RollingSuccessionPlanOverrideChecks
{
    public static void RollingSuccessionPlanOverrideStartsInCreated()
    {
        var item = RollingSuccessionPlanOverride.NewRollingSuccessionPlanOverride("code-822", "name-325");
        Check.Equal(item.Status, RollingSuccessionPlanOverrideStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingSuccessionPlanOverrideMovesToPublished()
    {
        var item = RollingSuccessionPlanOverride.NewRollingSuccessionPlanOverride("code-822", "name-325");
        try
        {
            item.MoveTo(RollingSuccessionPlanOverrideStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingSuccessionPlanOverrideStatus.Published, "status");
    }

    public static void RollingSuccessionPlanOverrideRejectsMovesFromRetired()
    {
        var item = RollingSuccessionPlanOverride.NewRollingSuccessionPlanOverride("code-822", "name-325");
        item.Status = RollingSuccessionPlanOverrideStatus.Retired;
        Check.Fails(() => item.MoveTo(RollingSuccessionPlanOverrideStatus.Created), "move");
    }

    public static void RunAll()
    {
        RollingSuccessionPlanOverrideStartsInCreated();
        RollingSuccessionPlanOverrideMovesToPublished();
        RollingSuccessionPlanOverrideRejectsMovesFromRetired();
    }
}
