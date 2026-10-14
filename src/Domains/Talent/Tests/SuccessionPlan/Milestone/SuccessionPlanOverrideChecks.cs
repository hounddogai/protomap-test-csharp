namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanOverrideChecks
{
    public static void SuccessionPlanOverrideStartsInDraft()
    {
        var item = SuccessionPlanOverride.NewSuccessionPlanOverride("code-926", "name-300");
        Check.Equal(item.Status, SuccessionPlanOverrideStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanOverrideMovesToClosed()
    {
        var item = SuccessionPlanOverride.NewSuccessionPlanOverride("code-926", "name-300");
        try
        {
            item.MoveTo(SuccessionPlanOverrideStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanOverrideStatus.Closed, "status");
    }

    public static void SuccessionPlanOverrideRejectsMovesFromClosed()
    {
        var item = SuccessionPlanOverride.NewSuccessionPlanOverride("code-926", "name-300");
        item.Status = SuccessionPlanOverrideStatus.Closed;
        Check.Fails(() => item.MoveTo(SuccessionPlanOverrideStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanOverrideStartsInDraft();
        SuccessionPlanOverrideMovesToClosed();
        SuccessionPlanOverrideRejectsMovesFromClosed();
    }
}
