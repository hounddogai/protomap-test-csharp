namespace Acme.Workforce.Absence.Tests.ReturnPlan.Audit;

using Acme.Workforce.Absence.ReturnPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ReturnPlanDraftChecks
{
    public static void ReturnPlanDraftStartsInIdle()
    {
        var item = ReturnPlanDraft.NewReturnPlanDraft("code-864", "label-671");
        Check.Equal(item.Status, ReturnPlanDraftStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanDraftMovesToWarming()
    {
        var item = ReturnPlanDraft.NewReturnPlanDraft("code-864", "label-671");
        try
        {
            item.MoveTo(ReturnPlanDraftStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanDraftStatus.Warming, "status");
    }

    public static void ReturnPlanDraftRejectsMovesFromStopped()
    {
        var item = ReturnPlanDraft.NewReturnPlanDraft("code-864", "label-671");
        item.Status = ReturnPlanDraftStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReturnPlanDraftStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReturnPlanDraftStartsInIdle();
        ReturnPlanDraftMovesToWarming();
        ReturnPlanDraftRejectsMovesFromStopped();
    }
}
