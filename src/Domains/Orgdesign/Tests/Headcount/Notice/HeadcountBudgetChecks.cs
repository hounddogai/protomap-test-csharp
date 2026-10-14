namespace Acme.Workforce.Orgdesign.Tests.Headcount.Notice;

using Acme.Workforce.Orgdesign.Headcount.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class HeadcountBudgetChecks
{
    public static void HeadcountBudgetStartsInNew()
    {
        var item = HeadcountBudget.NewHeadcountBudget("code-573", "title-524");
        Check.Equal(item.Status, HeadcountBudgetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountBudgetMovesToRunning()
    {
        var item = HeadcountBudget.NewHeadcountBudget("code-573", "title-524");
        try
        {
            item.MoveTo(HeadcountBudgetStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountBudgetStatus.Running, "status");
    }

    public static void HeadcountBudgetRejectsMovesFromFailed()
    {
        var item = HeadcountBudget.NewHeadcountBudget("code-573", "title-524");
        item.Status = HeadcountBudgetStatus.Failed;
        Check.Fails(() => item.MoveTo(HeadcountBudgetStatus.New), "move");
    }

    public static void RunAll()
    {
        HeadcountBudgetStartsInNew();
        HeadcountBudgetMovesToRunning();
        HeadcountBudgetRejectsMovesFromFailed();
    }
}
