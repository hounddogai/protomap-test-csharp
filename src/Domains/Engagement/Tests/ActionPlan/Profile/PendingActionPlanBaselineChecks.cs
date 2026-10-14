namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PendingActionPlanBaselineChecks
{
    public static void PendingActionPlanBaselineStartsInCreated()
    {
        var item = PendingActionPlanBaseline.NewPendingActionPlanBaseline("code-385", "name-465");
        Check.Equal(item.Status, PendingActionPlanBaselineStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingActionPlanBaselineMovesToValidated()
    {
        var item = PendingActionPlanBaseline.NewPendingActionPlanBaseline("code-385", "name-465");
        try
        {
            item.MoveTo(PendingActionPlanBaselineStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingActionPlanBaselineStatus.Validated, "status");
    }

    public static void PendingActionPlanBaselineRejectsMovesFromRetired()
    {
        var item = PendingActionPlanBaseline.NewPendingActionPlanBaseline("code-385", "name-465");
        item.Status = PendingActionPlanBaselineStatus.Retired;
        Check.Fails(() => item.MoveTo(PendingActionPlanBaselineStatus.Created), "move");
    }

    public static void RunAll()
    {
        PendingActionPlanBaselineStartsInCreated();
        PendingActionPlanBaselineMovesToValidated();
        PendingActionPlanBaselineRejectsMovesFromRetired();
    }
}
