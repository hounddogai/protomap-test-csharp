namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionBudgetChecks
{
    public static void ExceptionBudgetStartsInProposed()
    {
        var item = ExceptionBudget.NewExceptionBudget("code-807", "name-294");
        Check.Equal(item.Status, ExceptionBudgetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionBudgetMovesToRejected()
    {
        var item = ExceptionBudget.NewExceptionBudget("code-807", "name-294");
        try
        {
            item.MoveTo(ExceptionBudgetStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionBudgetStatus.Rejected, "status");
    }

    public static void ExceptionBudgetRejectsMovesFromWithdrawn()
    {
        var item = ExceptionBudget.NewExceptionBudget("code-807", "name-294");
        item.Status = ExceptionBudgetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ExceptionBudgetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ExceptionBudgetStartsInProposed();
        ExceptionBudgetMovesToRejected();
        ExceptionBudgetRejectsMovesFromWithdrawn();
    }
}
