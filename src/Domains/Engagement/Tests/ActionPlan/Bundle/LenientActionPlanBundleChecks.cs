namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LenientActionPlanBundleChecks
{
    public static void LenientActionPlanBundleStartsInDraft()
    {
        var item = LenientActionPlanBundle.NewLenientActionPlanBundle("code-249", "name-741");
        Check.Equal(item.Status, LenientActionPlanBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientActionPlanBundleMovesToPending()
    {
        var item = LenientActionPlanBundle.NewLenientActionPlanBundle("code-249", "name-741");
        try
        {
            item.MoveTo(LenientActionPlanBundleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientActionPlanBundleStatus.Pending, "status");
    }

    public static void LenientActionPlanBundleRejectsMovesFromClosed()
    {
        var item = LenientActionPlanBundle.NewLenientActionPlanBundle("code-249", "name-741");
        item.Status = LenientActionPlanBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientActionPlanBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientActionPlanBundleStartsInDraft();
        LenientActionPlanBundleMovesToPending();
        LenientActionPlanBundleRejectsMovesFromClosed();
    }
}
