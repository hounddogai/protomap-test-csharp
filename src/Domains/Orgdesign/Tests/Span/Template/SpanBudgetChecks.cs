namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SpanBudgetChecks
{
    public static void SpanBudgetStartsInNew()
    {
        var item = SpanBudget.NewSpanBudget("code-591", "name-370");
        Check.Equal(item.Status, SpanBudgetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanBudgetMovesToRunning()
    {
        var item = SpanBudget.NewSpanBudget("code-591", "name-370");
        try
        {
            item.MoveTo(SpanBudgetStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanBudgetStatus.Running, "status");
    }

    public static void SpanBudgetRejectsMovesFromFailed()
    {
        var item = SpanBudget.NewSpanBudget("code-591", "name-370");
        item.Status = SpanBudgetStatus.Failed;
        Check.Fails(() => item.MoveTo(SpanBudgetStatus.New), "move");
    }

    public static void RunAll()
    {
        SpanBudgetStartsInNew();
        SpanBudgetMovesToRunning();
        SpanBudgetRejectsMovesFromFailed();
    }
}
