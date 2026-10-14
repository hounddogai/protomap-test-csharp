namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class CappedCareerPathBudgetChecks
{
    public static void CappedCareerPathBudgetStartsInPlanned()
    {
        var item = CappedCareerPathBudget.NewCappedCareerPathBudget("code-540", "label-97");
        Check.Equal(item.Status, CappedCareerPathBudgetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedCareerPathBudgetMovesToCancelled()
    {
        var item = CappedCareerPathBudget.NewCappedCareerPathBudget("code-540", "label-97");
        try
        {
            item.MoveTo(CappedCareerPathBudgetStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedCareerPathBudgetStatus.Cancelled, "status");
    }

    public static void CappedCareerPathBudgetRejectsMovesFromCancelled()
    {
        var item = CappedCareerPathBudget.NewCappedCareerPathBudget("code-540", "label-97");
        item.Status = CappedCareerPathBudgetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CappedCareerPathBudgetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CappedCareerPathBudgetStartsInPlanned();
        CappedCareerPathBudgetMovesToCancelled();
        CappedCareerPathBudgetRejectsMovesFromCancelled();
    }
}
