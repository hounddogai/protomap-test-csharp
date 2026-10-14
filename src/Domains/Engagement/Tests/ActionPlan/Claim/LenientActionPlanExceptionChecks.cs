namespace Acme.Workforce.Engagement.Tests.ActionPlan.Claim;

using Acme.Workforce.Engagement.ActionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientActionPlanExceptionChecks
{
    public static void LenientActionPlanExceptionStartsInDraft()
    {
        var item = LenientActionPlanException.NewLenientActionPlanException("code-952", "label-870");
        Check.Equal(item.Status, LenientActionPlanExceptionStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientActionPlanExceptionMovesToActive()
    {
        var item = LenientActionPlanException.NewLenientActionPlanException("code-952", "label-870");
        try
        {
            item.MoveTo(LenientActionPlanExceptionStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientActionPlanExceptionStatus.Active, "status");
    }

    public static void LenientActionPlanExceptionRejectsMovesFromClosed()
    {
        var item = LenientActionPlanException.NewLenientActionPlanException("code-952", "label-870");
        item.Status = LenientActionPlanExceptionStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientActionPlanExceptionStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientActionPlanExceptionStartsInDraft();
        LenientActionPlanExceptionMovesToActive();
        LenientActionPlanExceptionRejectsMovesFromClosed();
    }
}
