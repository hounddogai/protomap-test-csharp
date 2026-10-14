namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanBatchChecks
{
    public static void ReturnPlanBatchStartsInRequested()
    {
        var item = ReturnPlanBatch.NewReturnPlanBatch("code-321", "label-494");
        Check.Equal(item.Status, ReturnPlanBatchStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanBatchMovesToGranted()
    {
        var item = ReturnPlanBatch.NewReturnPlanBatch("code-321", "label-494");
        try
        {
            item.MoveTo(ReturnPlanBatchStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanBatchStatus.Granted, "status");
    }

    public static void ReturnPlanBatchRejectsMovesFromExpired()
    {
        var item = ReturnPlanBatch.NewReturnPlanBatch("code-321", "label-494");
        item.Status = ReturnPlanBatchStatus.Expired;
        Check.Fails(() => item.MoveTo(ReturnPlanBatchStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ReturnPlanBatchStartsInRequested();
        ReturnPlanBatchMovesToGranted();
        ReturnPlanBatchRejectsMovesFromExpired();
    }
}
