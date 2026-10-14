namespace Acme.Workforce.Absence.Tests.ReturnPlan.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ReturnPlanWindowChecks
{
    public static void ReturnPlanWindowStartsInOpen()
    {
        var item = ReturnPlanWindow.NewReturnPlanWindow("code-197", "label-781");
        Check.Equal(item.Status, ReturnPlanWindowStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanWindowMovesToInReview()
    {
        var item = ReturnPlanWindow.NewReturnPlanWindow("code-197", "label-781");
        try
        {
            item.MoveTo(ReturnPlanWindowStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanWindowStatus.InReview, "status");
    }

    public static void ReturnPlanWindowRejectsMovesFromArchived()
    {
        var item = ReturnPlanWindow.NewReturnPlanWindow("code-197", "label-781");
        item.Status = ReturnPlanWindowStatus.Archived;
        Check.Fails(() => item.MoveTo(ReturnPlanWindowStatus.Open), "move");
    }

    public static void RunAll()
    {
        ReturnPlanWindowStartsInOpen();
        ReturnPlanWindowMovesToInReview();
        ReturnPlanWindowRejectsMovesFromArchived();
    }
}
