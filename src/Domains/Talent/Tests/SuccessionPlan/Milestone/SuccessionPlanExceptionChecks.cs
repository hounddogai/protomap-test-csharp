namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanExceptionChecks
{
    public static void SuccessionPlanExceptionStartsInDraft()
    {
        var item = SuccessionPlanException.NewSuccessionPlanException("code-152", "title-196");
        Check.Equal(item.Status, SuccessionPlanExceptionStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanExceptionMovesToSuspended()
    {
        var item = SuccessionPlanException.NewSuccessionPlanException("code-152", "title-196");
        try
        {
            item.MoveTo(SuccessionPlanExceptionStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanExceptionStatus.Suspended, "status");
    }

    public static void SuccessionPlanExceptionRejectsMovesFromClosed()
    {
        var item = SuccessionPlanException.NewSuccessionPlanException("code-152", "title-196");
        item.Status = SuccessionPlanExceptionStatus.Closed;
        Check.Fails(() => item.MoveTo(SuccessionPlanExceptionStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanExceptionStartsInDraft();
        SuccessionPlanExceptionMovesToSuspended();
        SuccessionPlanExceptionRejectsMovesFromClosed();
    }
}
