namespace Acme.Workforce.Absence.Tests.ReturnPlan.Summary;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReturnPlanSummaryChecks
{
    public static void ReturnPlanSummaryStartsInPlanned()
    {
        var item = ReturnPlanSummary.NewReturnPlanSummary("code-389", "name-184");
        Check.Equal(item.Status, ReturnPlanSummaryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanSummaryMovesToInProgress()
    {
        var item = ReturnPlanSummary.NewReturnPlanSummary("code-389", "name-184");
        try
        {
            item.MoveTo(ReturnPlanSummaryStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanSummaryStatus.InProgress, "status");
    }

    public static void ReturnPlanSummaryRejectsMovesFromCancelled()
    {
        var item = ReturnPlanSummary.NewReturnPlanSummary("code-389", "name-184");
        item.Status = ReturnPlanSummaryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ReturnPlanSummaryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ReturnPlanSummaryStartsInPlanned();
        ReturnPlanSummaryMovesToInProgress();
        ReturnPlanSummaryRejectsMovesFromCancelled();
    }
}
